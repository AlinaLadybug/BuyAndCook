using BuyAndCook.Application.Abstractions;
using Microsoft.JSInterop;

namespace BuyAndCook.Web.Services
{
    public class WebClipboardService : IClipboardService
    {
        private readonly IJSRuntime _jsRuntime;

        public WebClipboardService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetTextAsync(string text)
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
