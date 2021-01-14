using System;

namespace TestContainers.Images
{
    public class ImageDataBuilder
    {
        private DateTimeOffset _createdAt;

        internal ImageDataBuilder CreatedAt(DateTimeOffset createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        internal ImageData Build()
        {
            return new ImageData { CreatedAt = _createdAt };
        }
    }
}
