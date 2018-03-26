using System;
using System.IO;
using ImageResizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RedBit.XamServerless.Core;

namespace RedBit.XamServerless.Functions
{
    public static class ImageProcessingFunction
    {
        [FunctionName("ImageProcessingFunction")]
        public async static void Run([QueueTrigger("imageprocessingqueue", Connection = "AzureWebJobsStorage")]Core.ImageEntity imageEntity, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {imageEntity.RowKey}");

            // get the image stream from blob
            var ms = await BlobManager.Default.GetBlobAsStream(imageEntity.OriginalImageUrl);

            // get the current byte array image
            var imageBuilder = ImageResizer.ImageBuilder.Current;

            // TODO : can probably run this in parallel

            // create an extra small image
            var size = imageDimensionsTable[ImageSize.ExtraSmall];
            var newImage = new MemoryStream();
            imageBuilder.Build(
                ms, newImage,
                new ResizeSettings(size.Item1, size.Item2, FitMode.Max, null), false);
            imageEntity.ExtraSmallImageUrl = await BlobManager.Default.AddExtraSmallImage(newImage, imageEntity.OriginalImageUrl);

            // create a small image
            size = imageDimensionsTable[ImageSize.Small];
            newImage = new MemoryStream();
            imageBuilder.Build(
                ms, newImage,
                new ResizeSettings(size.Item1, size.Item2, FitMode.Max, null), false);
            imageEntity.SmallImageUrl = await BlobManager.Default.AddSmallImage(newImage, imageEntity.OriginalImageUrl);

            // create a medium image
            size = imageDimensionsTable[ImageSize.Medium];
            newImage = new MemoryStream();
            imageBuilder.Build(
                ms, newImage,
                new ResizeSettings(size.Item1, size.Item2, FitMode.Max, null), false);
            imageEntity.MediumImageUrl = await BlobManager.Default.AddMediumImage(newImage, imageEntity.OriginalImageUrl);

            // now update the table
            await TableManager.Default.UpdateRecord(imageEntity);
        }

        public enum ImageSize
        {
            ExtraSmall, Small, Medium
        }

        private static System.Collections.Generic.Dictionary<ImageSize, Tuple<int, int>> imageDimensionsTable = new System.Collections.Generic.Dictionary<ImageSize, Tuple<int, int>>()
        {
            { ImageSize.ExtraSmall, Tuple.Create(320, 200) },
            { ImageSize.Small,      Tuple.Create(640, 400) },
            { ImageSize.Medium,     Tuple.Create(800, 600) }
        };
    }
}
