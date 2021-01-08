using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Utility
{
    public abstract class ImageNameSubstitutor
    {
        private static readonly ILogger _logger = StaticLoggerFactory.CreateLogger<ImageNameSubstitutor>();
        public abstract Task<DockerImageName> Apply(DockerImageName original);

        protected abstract string Description { get; }

        internal static ImageNameSubstitutor _instance;

        internal static ImageNameSubstitutor _defaultImplementation = new DefaultImageNameSubstitutor();

        public static ImageNameSubstitutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    string configuredClassName = TestContainersConfiguration.Instance.ImageSubstitutorClassName;


                    if (configuredClassName != null)
                    {
                        _logger.LogDebug("Attempting to instantiate an ImageNameSubstitutor with class: {configuredClassName}", configuredClassName);
                        ImageNameSubstitutor configuredInstance;
                        try
                        {
                            configuredInstance = (ImageNameSubstitutor) Activator.CreateInstance(Type.GetType(configuredClassName));
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentException("Configured Image Substitutor could not be loaded: " + configuredClassName, e);
                        }

                        _logger.LogInformation("Found configured ImageNameSubstitutor: {configuredInstance}", configuredInstance.Description);

                        _instance = new ChainedImageNameSubstitutor(
                            WrapWithLogging(_defaultImplementation),
                            WrapWithLogging(configuredInstance)
                        );
                    }
                    else
                    {
                        _instance = WrapWithLogging(_defaultImplementation);
                    }

                    _logger.LogInformation("Image name substitution will be performed by: {instance}", _instance.Description);
                }
                return _instance;
            }
        }


        private static ImageNameSubstitutor WrapWithLogging(ImageNameSubstitutor wrappedInstance)
        {
            return new LogWrappedImageNameSubstitutor(wrappedInstance);
        }

        private class LogWrappedImageNameSubstitutor : ImageNameSubstitutor
        {
            ImageNameSubstitutor _wrappedInstance;

            public LogWrappedImageNameSubstitutor(ImageNameSubstitutor wrappedInstance)
            {
                _wrappedInstance = wrappedInstance;
            }

            public override async Task<DockerImageName> Apply(DockerImageName original)
            {
                var replacementImage = await _wrappedInstance.Apply(original);

                if (!replacementImage.Equals(original))
                {
                    _logger.LogInformation("Using {replacementImage} as a substitute image for {original} (using image substitutor: {substitutor})", replacementImage.AsCanonicalNameString(), original.AsCanonicalNameString(), _wrappedInstance.Description);
                    return replacementImage;
                }
                else
                {
                    _logger.LogDebug("Did not find a substitute image for {original} (using image substitutor: {substitutor})", original.AsCanonicalNameString(), _wrappedInstance.Description);
                    return original;
                }
            }

            protected override string Description => _wrappedInstance.Description;

        }

        /**
        * Wrapper substitutor that passes the original image name through a default substitutor and then the configured one
*/
        private class ChainedImageNameSubstitutor : ImageNameSubstitutor
        {
            private ImageNameSubstitutor _defaultInstance;
            private ImageNameSubstitutor _configuredInstance;

            public ChainedImageNameSubstitutor(ImageNameSubstitutor defaultInstance, ImageNameSubstitutor configuredInstance)
            {
                _defaultInstance = defaultInstance;
                _configuredInstance = configuredInstance;
            }

            public override async Task<DockerImageName> Apply(DockerImageName original)
            {
                return await _configuredInstance.Apply(await _defaultInstance.Apply(original));
            }


            protected override string Description =>
            String.Format(
                    "Chained substitutor of '%s' and then '%s'",
                    _defaultInstance.Description,
                    _configuredInstance.Description
                );

        }

    }

}
