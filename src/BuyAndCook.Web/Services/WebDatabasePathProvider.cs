using BuyAndCook.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BuyAndCook.Web.Services
{
    public class WebDatabasePathProvider : IDatabasePathProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string? _databasePath;

        public WebDatabasePathProvider(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _databasePath = configuration.GetValue<string>("Database:Path")
                ?? Environment.GetEnvironmentVariable("BUYANDCOOK_DB_PATH");
        }

        public string GetDatabasePath()
        {
            if (!string.IsNullOrWhiteSpace(_databasePath))
            {
                return _databasePath;
            }

            return Path.Combine(_environment.ContentRootPath, "App_Data", "buyandcook.db");
        }
    }
}
