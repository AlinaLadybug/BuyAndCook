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
                throw new FileNotFoundException("Recipe dataset not found.", fileInfo.PhysicalPath);
            }

            Stream stream = File.OpenRead(fileInfo.PhysicalPath!);
            return Task.FromResult(stream);
        }
    }
}
