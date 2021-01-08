using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TestContainers.Containers;
using TestContainers.Utility;

namespace TestContainers.Images
{
    public sealed class DockerImageName
    {
        /* Regex patterns used for validation */
        private static readonly string ALPHA_NUMERIC = "[a-z0-9]+";
        private static readonly string SEPARATOR = "([.]|_{1,2}|-+)";
        private static readonly string REPO_NAME_PART = ALPHA_NUMERIC + "(" + SEPARATOR + ALPHA_NUMERIC + ")*";
        private static readonly Regex REPO_NAME = new Regex(REPO_NAME_PART + "(/" + REPO_NAME_PART + ")*");

        public string Repository { get; }
        public Versioning Versioning { get; }

        private string _rawName;
        private DockerImageName _compatibleSubstituteFor;

        public string UnversionedPart
        {
            get
            {
                if (!"".Equals(Registry))
                {
                    return Registry + "/" + Repository;
                }
                else
                {
                    return Repository;
                }
            }
        }
        public string VersionPart => Versioning.ToString();

        public string Registry { get; internal set; }

        public static DockerImageName Parse(string fullImageName) => new DockerImageName(fullImageName);

        public DockerImageName WithTag(string tag)
        {
            throw new NotImplementedException();
        }

        private DockerImageName(string repository, string registry, Versioning versioning, DockerImageName compatibleSubstituteFor)
        {
            Repository = repository;
            Registry = registry;
            Versioning = versioning;
            _compatibleSubstituteFor = compatibleSubstituteFor;
        }

        internal bool IsCompatibleWith(DockerImageName other)
        {
            // is this image already the same or equivalent?
            if (other.Equals(this))
            {
                return true;
            }

            return _compatibleSubstituteFor?.IsCompatibleWith(other) == true;
        }

        public void AssertValid()
        {
            if (!IPEndPointParser.TryParse(Registry, out _))
            {
                throw new ArgumentException(Registry + "is not a valid registry host (in " + _rawName + ")");
            }
            //HostAndPort.FromString(Registry); // return value ignored - this throws if registry is not a valid host:port string
            if (!REPO_NAME.IsMatch(Repository))
            {
                throw new ArgumentException(Repository + " is not a valid Docker image name (in " + _rawName + ")");
            }
            if (!Versioning.IsValid())
            {
                throw new ArgumentException(Versioning + " is not a valid image versioning identifier (in " + _rawName + ")");
            }
        }

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
                Registry = "";
                remoteName = fullImageName;
            }
            else
            {
                Registry = fullImageName.Substring(0, slashIndex);
                remoteName = fullImageName.Substring(slashIndex + 1);
            }

            if (remoteName.Contains("@sha256:"))
            {
                var parts = Regex.Split(remoteName, "@sha256:");
                Repository = parts[0];
                Versioning = new Sha256Versioning(parts[1]);
            }
            else if (remoteName.Contains(":"))
            {
               Repository = remoteName.Split(':')[0];
               Versioning = new TagVersioning(remoteName.Split(':')[1]);
            }
            else
            {
                Repository = remoteName;
                Versioning = Versioning.ANY;
            }

            _compatibleSubstituteFor = null;
        }

        public DockerImageName WithRegistry(string registry)
        {
            return new DockerImageName(Repository, registry, Versioning, _compatibleSubstituteFor);
        }

        public DockerImageName WithRepository(string repository)
        {
            return new DockerImageName(repository, Registry, Versioning, _compatibleSubstituteFor);
        }

        public DockerImageName WithCompatibleSubstituteFor(DockerImageName compatibleSubstituteFor)
        {
            return new DockerImageName(Repository, Registry, Versioning, compatibleSubstituteFor);
        }

        public string AsCanonicalNameString() => UnversionedPart + Versioning.Separator + VersionPart;

        public override string ToString() => AsCanonicalNameString();


        /**
    * Declare that this {@link DockerImageName} is a compatible substitute for another image - i.e. that this image
    * behaves as the other does, and is compatible with Testcontainers' assumptions about the other image.
    *
    * @param otherImageName the image name of the other image
    * @return an immutable copy of this {@link DockerImageName} with the compatibility declaration attached.
    */
        public DockerImageName AsCompatibleSubstituteFor(String otherImageName)
        {
            return WithCompatibleSubstituteFor(DockerImageName.Parse(otherImageName));
        }

        /**
         * Declare that this {@link DockerImageName} is a compatible substitute for another image - i.e. that this image
         * behaves as the other does, and is compatible with Testcontainers' assumptions about the other image.
         *
         * @param otherImageName the image name of the other image
         * @return an immutable copy of this {@link DockerImageName} with the compatibility declaration attached.
         */
        public DockerImageName AsCompatibleSubstituteFor(DockerImageName otherImageName)
        {
            return WithCompatibleSubstituteFor(otherImageName);
        }

        /**
     * Behaves as {@link DockerImageName#isCompatibleWith(DockerImageName)} but throws an exception
     * rather than returning false if a mismatch is detected.
     *
     * @param anyOthers the other image(s) that we are trying to check compatibility with. If more
     *                  than one is provided, this method will check compatibility with at least one
     *                  of them.
     * @throws IllegalStateException if {@link DockerImageName#isCompatibleWith(DockerImageName)}
     *                               returns false
     */
        public void AssertCompatibleWith(params DockerImageName[] anyOthers)
        {
            if (anyOthers.Length == 0)
            {
                throw new ArgumentException("parameter must be non-empty", nameof(anyOthers));
            }

            foreach (var anyOther in anyOthers)
            {
                if (IsCompatibleWith(anyOther))
                {
                    return;
                }
            }

            var exampleOther = anyOthers[0];

            throw new IllegalStateException(
                
                    $"Failed to verify that image '{_rawName}' is a compatible substitute for '{exampleOther._rawName}'. This generally means that "
                        +
                        "you are trying to use an image that Testcontainers has not been designed to use. If this is "
                        +
                        "deliberate, and if you are confident that the image is compatible, you should declare "
                        +
                        $"compatibility in code using the `{nameof(AsCompatibleSubstituteFor)}` method. For example:\n"
                        +
                        $"   var myImage = {nameof(DockerImageName)}.{nameof(Parse)}(\"{_rawName}\").{nameof(AsCompatibleSubstituteFor)}(\"{exampleOther._rawName}\");\n"
                        +
                        "and then use `myImage` instead."
                
            );
        }

        public override bool Equals(object obj)
        {
            return obj is DockerImageName name &&
                   Repository == name.Repository &&
                   EqualityComparer<Versioning>.Default.Equals(Versioning, name.Versioning) &&
                   Registry == name.Registry;
        }

        public override int GetHashCode()
        {
            int hashCode = 1594136807;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Repository);
            hashCode = hashCode * -1521134295 + EqualityComparer<Versioning>.Default.GetHashCode(Versioning);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Registry);
            return hashCode;
        }
    }
}
