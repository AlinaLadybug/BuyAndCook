using BuyAndCook.Application.Abstractions;
using Microsoft.Maui.Storage;

namespace BuyAndCook.Data
{
    public class MauiRecipeDataSource : IRecipeDataSource
    {
        private const string DataFileName = "recipes.json";

        public Task<Stream> OpenStreamAsync()
        {
            return FileSystem.OpenAppPackageFileAsync(DataFileName);
        }
    }
}
