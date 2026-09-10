using DnsChanger.Data;
using DnsChanger.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Repository
{
    public class CustomDnsRepository : ICustomDnsRepository
    {
        public List<CustomDnsEntry> GetAllDns()
        {
            var result = new List<CustomDnsEntry>();

            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, PrimaryDns, SecondaryDns, CreatedAt FROM CustomDnsEntries ORDER BY CreatedAt DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new CustomDnsEntry
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Primary = reader.GetString(2),
                    Secondary = reader.IsDBNull(3) ? null: reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4))
                });
            }
            return result;
        }
        public void Add(CustomDnsEntry entry)
        {
            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO CustomDnsEntries (Name, PrimaryDns, SecondaryDns, CreatedAt) 
                VALUES ($name, $primary, $secondary, $createdAt);";

            command.Parameters.AddWithValue("$name", entry.Name);
            command.Parameters.AddWithValue("$primary", entry.Primary);
            command.Parameters.AddWithValue("$secondary", (object)entry.Secondary ?? DBNull.Value);
            command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("o")); // Use ISO 8601 format

            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CustomDnsEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}
