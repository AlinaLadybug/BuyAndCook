using BuyAndCook.Application.Abstractions;
using Microsoft.Maui.ApplicationModel;

namespace BuyAndCook.PlatformServices
{
    public class MauiExternalLinkService : IExternalLinkService
    {
        public Task OpenAsync(string url)
        {
            return Launcher.Default.OpenAsync(url);
        }
    }
}
