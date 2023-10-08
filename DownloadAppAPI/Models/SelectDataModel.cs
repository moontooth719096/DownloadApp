using YoutubeExplode.Videos;

namespace DownloadAppAPI.Models
{
    public class SelectDataModel
    {
        public string id { get; set; }
        public VideoId VideoId => VideoId.Parse(id);
        public string Title { get; set; }
    }
}
