namespace BuyAndCook.Application.Abstractions
{
    public interface IExternalLinkService
    {
        Task OpenAsync(string url);
    }
}
