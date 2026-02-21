using BuyAndCook.Application.Abstractions;
using System.IO;

namespace BuyAndCook.Web.Services
{
    public class WebDatabasePathProvider : IDatabasePathProvider
    {
        private readonly IWebHostEnvironment _environment;

        public WebDatabasePathProvider(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string GetDatabasePath()
        {
            return Path.Combine(_environment.ContentRootPath, "App_Data", "buyandcook.db");
        }
    }
}
