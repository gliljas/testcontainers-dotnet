using System;
using System.Collections.Generic;
using System.Text;
using TestContainers.Images;

namespace TestContainers.Tests
{
    public static class TestImages
    {
        public static DockerImageName REDIS_IMAGE = DockerImageName.Parse("redis:3.0.2");
        public static DockerImageName RABBITMQ_IMAGE = DockerImageName.Parse("rabbitmq:3.5.3");
        public static DockerImageName MONGODB_IMAGE = DockerImageName.Parse("mongo:3.1.5");
        public static DockerImageName ALPINE_IMAGE = DockerImageName.Parse("alpine:3.2");
        public static DockerImageName DOCKER_REGISTRY_IMAGE = DockerImageName.Parse("registry:2.7.0");
        public static DockerImageName TINY_IMAGE = DockerImageName.Parse("alpine:3.5");
    }
}
