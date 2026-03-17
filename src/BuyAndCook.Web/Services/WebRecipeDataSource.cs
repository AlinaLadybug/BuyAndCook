using BuyAndCook.Application.Abstractions;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace BuyAndCook.Web.Services
{
    public class WebRecipeDataSource : IRecipeDataSource
    {
        private readonly IWebHostEnvironment _environment;

        public WebRecipeDataSource(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task<Stream> OpenStreamAsync()
        {
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo("data/recipes.json");
            if (!fileInfo.Exists)
            {
                var fallbackPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "data", "recipes.json");
                if (File.Exists(fallbackPath))
                {
                    return Task.FromResult<Stream>(File.OpenRead(fallbackPath));
                }

                throw new FileNotFoundException("Recipe dataset not found.", fallbackPath);
            }

            Stream stream = File.OpenRead(fileInfo.PhysicalPath!);
            return Task.FromResult(stream);
        }
    }
}
