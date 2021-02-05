using Serilog;
using System.Diagnostics;
using System.Threading;

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

            Log.Information($"Ctor [YTVideo] YTVideo() Thread:{Thread.CurrentThread.ManagedThreadId}");

            Title = videoTitle;
            Published = videoPublished;
            Channel = videoChannel;
            VideoLength = videoLength;
            ViewsCount = videoViews;
            Link = videoLink;

            Log.Debug($"Fin [YTVideo] YTVideo() Thread:{Thread.CurrentThread.ManagedThreadId} \"init repo\"");
        }
    }
}
