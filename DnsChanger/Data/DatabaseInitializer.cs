using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Data
{
    public class DatabaseInitializer
    {
        public static string DbPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DnsChanger", "dnschanger.db");

        public static void Initialize()
        {
            var directory = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS CustomDnsEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    PrimaryDns TEXT NOT NULL,
                    SecondaryDns TEXT NOT NULL,
                    CreatedAt DATETIME NOT NULL
                );";
            command.ExecuteNonQuery();
        }
    }
}
