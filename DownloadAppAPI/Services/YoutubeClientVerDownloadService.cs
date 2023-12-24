using DownloadAppAPI.Interfaces;
using DownloadAppAPI.Models;
using DownloadAppAPI.SignalRHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NAudio.Wave;
using NReco.VideoConverter;
using System.IO.Compression;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DownloadAppAPI.Services
{
    public class YoutubeClientVerDownloadService : YoutubeDonloadBaseService, IYoutubeListDownloadService
    {
        private readonly YoutubeClient _youtubeclient;
        private readonly IHubContext<YoutubeDownloadProgressHub> _youtubeDownloadProgressHub;
        public YoutubeClientVerDownloadService(IHubContext<YoutubeDownloadProgressHub> youtubeDownloadProgressHub)
        {
            _youtubeDownloadProgressHub = youtubeDownloadProgressHub;
            _youtubeclient = new YoutubeClient();
        }

        public async IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId)
        {
            //取得youtube清單
            IEnumerable<PlaylistVideo> MusicList = await SearchListYoutubeClientVer_Get(PlaylistId);
            if (MusicList != null)
            {
                using (var enumerator = MusicList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PlaylistVideo value = enumerator.Current;
                        yield return new YotubeDownloadListViewModel
                        {
                            IsCheck = true,
                            Title = value.Title,
                            Id = value.Id,
                            ThumbnailUrl = value.Thumbnails.SingleOrDefault(Thumbnail => Thumbnail.Resolution.Area == value.Thumbnails.Max(Thumbnail => Thumbnail.Resolution.Area)).Url,
                            PlayTime = value.Duration.ToString()
                        };
                    }
                }
            }
        }
        public async Task<IActionResult> DownloadApp(IEnumerable<SelectDataModel> SelectData)
        {
            APIResponseModel result = new APIResponseModel { Code = 1 };
            List<Task> downloadList = new List<Task>();
            List<SelectDataModel> Dolist = SelectData.OrderBy(x => x.id).ToList();
            string Token = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string folderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "YoutubeDonload", Token);

            // 检查文件夹是否存在
            FileCheck(folderPath);

            int Takecount = 5;
            double TotalCount = Dolist.Count();
            string message = $"音樂下載中";
            double percentage = 0;
            
            while (Dolist.Count() > 0)
            {
                IEnumerable<SelectDataModel> nowlist = Dolist.Take(Takecount);
                Action updateProgress = async () =>
                {
                    percentage = 100 - (Math.Round((Dolist.Count() / TotalCount) * 100));
                    await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);
                };
                foreach (var searchResult in nowlist)
                {
                    
                    string filename = $"{searchResult.Title}.mp4";
                    
                    //移除windows不允許的檔名字元
                    filename = Regex.Replace(filename, "[\\/:*?\"<>|]", "");
                    // 保存 MP3 文件
                    //var outPath = Path.Combine(folderPath, filename);
                    filename = filename.Replace(" ", "");

                    downloadList.Add(Task.Run(async () => { 
                        await ConvertToMP3(searchResult.VideoId, folderPath, filename);
                        updateProgress();
                    }));

                }
      
                await Task.WhenAll(downloadList);
                Dolist.RemoveRange(0, nowlist.Count());
                //downloadList.EX.Exception.InnerExceptions.Select(o => o.Message).ToArray());

            }

            percentage = 100;
            await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);

            return await ZipDownloadFile(Token, folderPath);
        }
        private async Task<IActionResult> ZipDownloadFile(string Token, string tragePath)
        {
            string zipFileName = $"compressed-files-{Token}.zip";
            string zipFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "YoutubeDonloadZIP");
            string AllzipFilePath = Path.Combine(zipFilePath, zipFileName);

            // 检查文件夹是否存在
            FileCheck(zipFilePath);

            // 压缩临时文件夹中的所有文件
            ZipFile.CreateFromDirectory(tragePath, AllzipFilePath);

            // 构建响应，将zip文件提供给客户端下载
            var memoryStream = new MemoryStream();
            using (var fs = new FileStream(AllzipFilePath, FileMode.Open))
            {
                await fs.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(memoryStream, System.Net.Mime.MediaTypeNames.Application.Octet);

        }

        private async Task<IEnumerable<PlaylistVideo>> SearchListYoutubeClientVer_Get(string PlaylistId)
        {
            var playlistUrl = "https://youtube.com/playlist?list=" + PlaylistId;

            return await _youtubeclient.Playlists.GetVideosAsync(playlistUrl);
        }
        private async Task ConvertToMP3(VideoId item, string folderPath, string filename)
        {
            try
            {
                // 解析影片信息
                var video = await _youtubeclient.Videos.Streams.GetManifestAsync(item);

                // 擷取聲音(取最高音質
                var audioStreamInfo = video.GetAudioOnlyStreams().GetWithHighestBitrate();
                //var audioStreamInfo = video.GetAudioStreams().GetWithHighestBitrate();
                //var audioStreamInfo = video.GetAudioOnlyStreams().Where(x=>x.Container == Container.Mp4).GetWithHighestBitrate();

                //設定要轉換的kbps
                int nowkbps = kbpsSet(audioStreamInfo.Bitrate.KiloBitsPerSecond);


                //取得stream
                var audioStream = await _youtubeclient.Videos.Streams.GetAsync(audioStreamInfo);
                //string  MP4outPath = Path.Combine(folderPath, filename);
                //outPath = outPath.Replace(".mp3", ".mp4");
                //下載
                //await _youtubeclient.Videos.Streams.DownloadAsync(audioStreamInfo, MP4outPath);
                
                var Convert = new NReco.VideoConverter.FFMpegConverter();
                String SaveMP3File = filename.Replace(".mp4", ".mp3");
                string MP3outPath = Path.Combine(folderPath, SaveMP3File);
                var settings = new ConvertSettings
                {
                    AudioCodec = "mp3",
                    CustomOutputArgs = $"-b:a {nowkbps}k"
                };

                try
                {
                     Convert.ConvertLiveMedia(audioStream, null, MP3outPath, null, settings).Start();
                    //Convert.ConvertMedia(MP4outPath, null, MP3outPath, null, settings);
                }
                catch (Exception ex)
                {

                }
                //File.Delete(MP4outPath);
               
                await Console.Out.WriteLineAsync($"已保存 MP3 文件至 {MP3outPath}。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下載 {filename} 發生錯誤");
            }
        }

        private int kbpsSet(double orgkbps)
        {
            int nowkbps = 0;
            //設定要轉換的kbps
            switch (orgkbps)
            {
                case <= 96://64kbps
                    nowkbps = 64;
                    break;
                case > 96 and <= 145://128kbps
                    nowkbps = 128;
                    break;
                case > 145 and <= 224://192kbps
                    nowkbps = 192;
                    break;
                case > 224 and <= 278://256kbps
                    nowkbps = 256;
                    break;
                case > 278://320kbps
                    nowkbps = 320;
                    break;
            }
            return nowkbps;
        }
    }
}
