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

            if (File.Exists(_databasePath))
            {
                var attributes = File.GetAttributes(_databasePath);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(_databasePath, attributes & ~FileAttributes.ReadOnly);
                }
            }
        }

        public SqliteConnection CreateConnection()
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            };
            var connection = new SqliteConnection(builder.ToString());
            connection.Open();
            return connection;
        }
    }
}
