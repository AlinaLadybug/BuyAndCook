using BuyAndCook.Application.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BuyAndCook.Web.Services
{
    public class WebExternalLinkService : IExternalLinkService
    {
        private readonly NavigationManager _navigationManager;

        public WebExternalLinkService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public Task OpenAsync(string url)
        {
            _navigationManager.NavigateTo(url, forceLoad: true);
            return Task.CompletedTask;
        }
    }
}
