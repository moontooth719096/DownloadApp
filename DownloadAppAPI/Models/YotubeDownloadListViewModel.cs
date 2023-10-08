using YoutubeExplode.Common;

namespace DownloadAppAPI.Models
{
    public class YotubeDownloadListViewModel
    {
        public bool IsCheck { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public string Url => "https://www.youtube.com/watch?v=" + Id;
        public string ThumbnailUrl { get; set; }
        public string PlayTime { get; set; }
    }
}
