using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Utility
{
    //public interface IImageNameSubstitutor
    //{
    //}

    //public delegate DockerImageName ImageNameSubstitution(DockerImageName dockerImageName);

    public abstract class ImageNameSubstitutor
    {
        private static ILogger _logger;
        public abstract DockerImageName Apply(DockerImageName original);

        protected abstract string Description { get; }

        public static ImageNameSubstitutor Instance => null;

        static ImageNameSubstitutor _defaultImplementation = new DefaultImageNameSubstitutor();

        private static ImageNameSubstitutor CreateDefault()
        {
            string configuredClassName = TestContainersConfiguration.Instance.ImageSubstitutorClassName;

            ImageNameSubstitutor instance;

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

                instance = new ChainedImageNameSubstitutor(
                    WrapWithLogging(_defaultImplementation),
                    WrapWithLogging(configuredInstance)
                );
            }
            else
            {
                instance = WrapWithLogging(_defaultImplementation);
            }

            _logger.LogInformation("Image name substitution will be performed by: {instance}", instance.Description);
            return instance;
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

            public override DockerImageName Apply(DockerImageName original)
            {
                var replacementImage = _wrappedInstance.Apply(original);

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

            public override DockerImageName Apply(DockerImageName original)
            {
                return _configuredInstance.Apply(_defaultInstance.Apply(original));
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
