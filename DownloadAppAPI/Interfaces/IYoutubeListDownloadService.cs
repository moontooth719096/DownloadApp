using DownloadAppAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DownloadAppAPI.Interfaces
{
    public interface IYoutubeListDownloadService
    {
        IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId);
        Task<IActionResult> DownloadApp(IEnumerable<SelectDataModel> SelectData);
    }
}