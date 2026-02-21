namespace BuyAndCook.Application.Abstractions
{
    public interface IClipboardService
    {
        Task SetTextAsync(string text);
    }
}
