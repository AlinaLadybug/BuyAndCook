using BuyAndCook.Application.Abstractions;
using Microsoft.Maui.Storage;
using System.IO;

namespace BuyAndCook.PlatformServices
{
    public class MauiDatabasePathProvider : IDatabasePathProvider
    {
        public string GetDatabasePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "buyandcook.db");
        }
    }
}
