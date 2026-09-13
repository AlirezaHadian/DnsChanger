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
                    SecondaryDns TEXT,
                    CreatedAt TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM CustomDnsEntries";
            long count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                var seedData = new (string Name, string Primary, string Secondary)[]
                {
                    ("Shecan", "178.22.122.100", "185.51.200.2"),
                    ("Bogzar", "185.55.226.26", "185.55.225.25"),
                    ("403", "10.202.10.202", "10.202.10.102"),
                };

                foreach (var (name, primary, secondary) in seedData)
                {
                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = @"
                    INSERT INTO CustomDnsEntries (Name, PrimaryDns, SecondaryDns, CreatedAt)
                    VALUES ($name, $primary, $secondary, $createdAt);";
                    insertCommand.Parameters.AddWithValue("$name", name);
                    insertCommand.Parameters.AddWithValue("$primary", primary);
                    insertCommand.Parameters.AddWithValue("$secondary", secondary);
                    insertCommand.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("o"));
                    insertCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
