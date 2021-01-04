using System;
using Docker.DotNet.Models;

namespace TestContainers.Images
{
    public class ImageData
    {
        internal static ImageData From(ImageInspectResponse response)
        {
            throw new NotImplementedException();
        }

        internal static ImageData From(ImagesListResponse image)
        {
            throw new NotImplementedException();
        }
    }
}
