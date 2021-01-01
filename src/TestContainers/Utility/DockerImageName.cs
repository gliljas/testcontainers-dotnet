using System;
using System.Text.RegularExpressions;
using TestContainers.Utility;

namespace TestContainers.Images
{
    public sealed class DockerImageName
    {
        private readonly Versioning _versioning;
        private object _registry;
        private string _repository;
        private string _rawName;
        private DockerImageName _compatibleSubstituteFor;

        public string UnversionedPart
        {
            get
            {
                if (!"".Equals(_registry))
                {
                    return _registry + "/" + _repository;
                }
                else
                {
                    return _repository;
                }
            }
        }
        public string VersionPart => _versioning.ToString();

        public static DockerImageName Parse(string fullImageName) => new DockerImageName(fullImageName);

        private DockerImageName(string fullImageName)
        {
            _rawName = fullImageName;
            var slashIndex = fullImageName.IndexOf('/');

            string remoteName;
            if (slashIndex == -1 ||
                (!fullImageName.Substring(0, slashIndex).Contains(".") &&
                    !fullImageName.Substring(0, slashIndex).Contains(":") &&
                    !fullImageName.Substring(0, slashIndex).Equals("localhost")))
            {
                _registry = "";
                remoteName = fullImageName;
            }
            else
            {
                _registry = fullImageName.Substring(0, slashIndex);
                remoteName = fullImageName.Substring(slashIndex + 1);
            }

            if (remoteName.Contains("@sha256:"))
            {
                var parts = Regex.Split(remoteName, "@sha256:");
                _repository = parts[0];
                _versioning = new Sha256Versioning(parts[1]);
            }
            else if (remoteName.Contains(":"))
            {
               _repository = remoteName.Split(':')[0];
               _versioning = new TagVersioning(remoteName.Split(':')[1]);
            }
            else
            {
                _repository = remoteName;
                _versioning = Versioning.ANY;
            }

            _compatibleSubstituteFor = null;
        }

        public string AsCanonicalNameString() => UnversionedPart + _versioning.Separator + VersionPart;

        public override string ToString() => AsCanonicalNameString();
        
    }
}
