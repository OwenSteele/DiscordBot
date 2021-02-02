namespace RhythmHelper
{
    public class YTVideo
    {
        public string Title { get; set; }
        public string Published { get; set; }
        public string Channel { get; set; }
        public string VideoLength { get; set; }
        public string ViewsCount { get; set; }
        public string Link { get; set; }

        public YTVideo(
            string videoTitle, string videoPublished,
            string videoChannel, string videoLength,
            string videoViews, string videoLink)
        {
            Title = videoTitle;
            Published = videoPublished;
            Channel = videoChannel;
            VideoLength = videoLength;
            ViewsCount = videoViews;
            Link = videoLink;
        }
    }
}
