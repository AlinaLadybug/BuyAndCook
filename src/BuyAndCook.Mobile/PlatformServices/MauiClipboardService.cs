using BuyAndCook.Application.Abstractions;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace BuyAndCook.PlatformServices
{
    public class MauiClipboardService : IClipboardService
    {
        public Task SetTextAsync(string text)
        {
            return Clipboard.Default.SetTextAsync(text);
        }
    }
}
