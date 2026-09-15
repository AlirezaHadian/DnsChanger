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
    public class ActivityLogRepository : IActivityLogRepository
    {
        public List<ActivityLogEntry> GetRecent(int count = 100)
        {
            var result = new List<ActivityLogEntry>();

            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Details, Timestamp FROM ActivityLog ORDER BY Timestamp DESC LIMIT $count;";
            command.Parameters.AddWithValue("$count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ActivityLogEntry
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Details = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Timestamp = DateTime.Parse(reader.GetString(3))
                });
            }
            return result;
        }

        public void Add(string title, string details = null)
        {
            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ActivityLog (Title, Details, Timestamp)
                VALUES ($title, $details, $timestamp);";
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$details", (object)details ?? DBNull.Value);
            command.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("o"));
            command.ExecuteNonQuery();
        }

        public void DeleteAll()
        {
            using var connection = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ActivityLog;";
            command.ExecuteNonQuery();
        }
    }
}