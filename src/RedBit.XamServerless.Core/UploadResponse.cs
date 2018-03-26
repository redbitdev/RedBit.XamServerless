
namespace RedBit.XamServerless.Core
{
    public class UploadResponse
    {
        public string Url { get; set; }
        public string Id { get; set; }
        public Images Images { get; set; }
    }

    public class Images
    {
        public string OriginalImageUrl { get; set; }
        public string ExtraSmallImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
    }
}