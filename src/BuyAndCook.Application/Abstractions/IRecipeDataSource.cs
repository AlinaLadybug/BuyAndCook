using System.IO;

namespace BuyAndCook.Application.Abstractions
{
    public interface IRecipeDataSource
    {
        Task<Stream> OpenStreamAsync();
    }
}
