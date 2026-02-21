using BuyAndCook.Application.Abstractions;
using Microsoft.Data.Sqlite;
using System.IO;

namespace BuyAndCook.Infrastructure.Persistence
{
    public class SqliteConnectionFactory
    {
        private readonly string _databasePath;

        public SqliteConnectionFactory(IDatabasePathProvider pathProvider)
        {
            _databasePath = pathProvider.GetDatabasePath();
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();
            return connection;
        }
    }
}
