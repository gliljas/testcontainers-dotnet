//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Microsoft.Extensions.Logging;
//using YamlDotNet.RepresentationModel;
//using YamlDotNet.Serialization;

//namespace TestContainers.Containers
//{
//    internal class ParsedDockerComposeFile
//    {
//        private ILogger _logger;
//        private readonly Dictionary<string, object> _composeFileContent;
//        private readonly string _composeFileName;
//        private readonly FileInfo _composeFile;

//        private ISet<string> _dependencyImageNames = new HashSet<string>();

//        ParsedDockerComposeFile(FileInfo composeFile)
//        {
//            var deserializer = new DeserializerBuilder()
//    .Build();


//            var yaml = File.ReadAllText(composeFile.FullName);

//            _composeFileContent = deserializer.Deserialize<Dictionary<string, object>>(yaml);
//            /*catch (Exception e)
//            {
//                throw new IllegalArgumentException("Unable to parse YAML file from " + composeFile.getAbsolutePath(), e);
//            }*/
//            _composeFileName = composeFile.FullName;
//            _composeFile = composeFile;
//            ParseAndValidate();
//        }

//        internal ParsedDockerComposeFile(Dictionary<string, object> testContent)
//        {
//            _composeFileContent = testContent;
//            _composeFileName = "";
//            _composeFile = new FileInfo(".");

//            ParseAndValidate();
//        }

//        private void ParseAndValidate()
//        {
//            IDictionary<string, object> servicesMap;
//            if (_composeFileContent.TryGetValue("version", out var version))
//            {
//                if ("2.0".Equals(version))
//                {
//                    _logger.LogWarning("Testcontainers may not be able to clean up networks spawned using Docker Compose v2.0 files. " +
//                        "Please see https://github.com/testcontainers/moby-ryuk/issues/2, and specify 'version: \"2.1\"' or " +
//                        "higher in {composeFileName}", _composeFileName);
//                }


//                if (!_composeFileContent.TryGetValue("services", out var servicesElement))
//                {
//                    _logger.LogDebug("Compose file {composeFileName} has an unknown format: 'version' is set but 'services' is not defined", _composeFileName);
//                    return;
//                }
//                if (!(servicesElement is IDictionary<string, object> servicesMapFromElement))
//                {
//                    _logger.LogDebug("Compose file {} has an unknown format: 'services' is not Map", _composeFileName);
//                    return;
//                }

//                servicesMap = servicesMapFromElement;
//            }
//            else
//            {
//                servicesMap = _composeFileContent;
//            }

//            foreach (var entry in servicesMap)
//            {
//                var serviceName = entry.Key;
//                var serviceDefinition = entry.Value;
//                if (!(serviceDefinition is IDictionary))
//                {
//                    _logger.LogDebug("Compose file {composeFileName} has an unknown format: service '{serviceName}' is not Map", _composeFileName, serviceName);
//                    break;
//                }
//                var serviceDefinitionMap = (IDictionary) serviceDefinition;

//                ValidateNoContainerNameSpecified(serviceName, serviceDefinitionMap);
//                FindServiceImageName(serviceDefinitionMap);
//                FindImageNamesInDockerfile(serviceDefinitionMap);
//            }


//        }

//        private void ValidateNoContainerNameSpecified(string serviceName, IDictionary serviceDefinitionMap)
//        {
//            if (serviceDefinitionMap.Contains("container_name"))
//            {
//                throw new IllegalStateException(string.Format(
//                    "Compose file %s has 'container_name' property set for service '%s' but this property is not supported by Testcontainers, consider removing it",
//                    _composeFileName,
//                    serviceName
//                ));
//            }
//        }

//        private void FindServiceImageName(IDictionary serviceDefinitionMap)
//        {
//            if (serviceDefinitionMap.Contains("image") && serviceDefinitionMap["image"] is string imageName)
//            {
//                _logger.LogDebug("Resolved dependency image for Docker Compose in {composeFileName}: {imageName}", _composeFileName, imageName);
//                _dependencyImageNames.Add(imageName);
//            }
//        }

//        private void FindImageNamesInDockerfile(IDictionary serviceDefinitionMap)
//        {
//            var buildNode = serviceDefinitionMap["build"];
//            string dockerfilePath = null;

//            if (buildNode is IDictionary<string, object> buildElement)
//            {
//                final Object dockerfileRelativePath = buildElement.get("dockerfile");
//                final Object contextRelativePath = buildElement.get("context");
//                if (dockerfileRelativePath instanceof String && contextRelativePath instanceof String) {
//                    dockerfilePath = composeFile
//                        .getParentFile()
//                        .toPath()
//                        .resolve((String) contextRelativePath)
//                        .resolve((String) dockerfileRelativePath)
//                        .normalize();
//                }
//            }
//            else if (buildNode is string)
//            {
//                dockerfilePath = _composeFile
//                    .getParentFile()
//                    .toPath()
//                    .resolve((String) buildNode)
//                    .resolve("./Dockerfile")
//                    .normalize();
//            }

//            if (dockerfilePath != null && File.Exists(dockerfilePath))
//            {
//                var resolvedImageNames = new ParsedDockerfile(dockerfilePath).DependencyImageNames;
//                if (resolvedImageNames.Any())
//                {
//                    _logger.LogDebug("Resolved Dockerfile dependency images for Docker Compose in {composeFileName} -> {dockerfilePath}: {resolvedImageNames}", _composeFileName, dockerfilePath, resolvedImageNames);
//                    _dependencyImageNames.UnionWith(resolvedImageNames);
//                }
//            }
//        }
//    }
//}
