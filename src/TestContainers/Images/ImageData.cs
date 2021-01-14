using System;
using Docker.DotNet.Models;

namespace TestContainers.Images
{
    public class ImageData
    {
        public static ImageDataBuilder Builder => new ImageDataBuilder();
        public DateTimeOffset CreatedAt { get; internal set; }

        internal static ImageData From(ImageInspectResponse response)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            return new ImageData { CreatedAt = response.Created };
        }

        internal static ImageData From(ImagesListResponse image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return new ImageData { CreatedAt = image.Created };
        }
    }
}
