using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Data
{
    public class DatabaseHelper
    {
        private const string Server = "localhost";
        private const string Database = "cybersecurity_bot";
        private const string User = "root";
        private const string Password = "Nekhwevha20";

        private readonly string _connectionString =
            $"Server=localhost;Database=cybersecurity_bot;Uid=root;Pwd=Nekhwevha20;";

        public void InitialiseDatabase()
        {
            string rootConn = "Server=localhost;Uid=root;Pwd=Nekhwevha20;";
            using var con = new MySqlConnection(rootConn);
            con.Open();

            new MySqlCommand("CREATE DATABASE IF NOT EXISTS `cybersecurity_bot`;", con)
                .ExecuteNonQuery();
            new MySqlCommand("USE `cybersecurity_bot`;", con)
                .ExecuteNonQuery();
            new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS cyber_tasks (
                    id            INT AUTO_INCREMENT PRIMARY KEY,
                    title         VARCHAR(255) NOT NULL,
                    description   TEXT,
                    is_completed  TINYINT(1)   NOT NULL DEFAULT 0,
                    reminder_date DATETIME     NULL,
                    created_at    DATETIME     NOT NULL
                );", con).ExecuteNonQuery();
        }

        public int AddTask(TaskItem task)
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var cmd = new MySqlCommand(@"
                INSERT INTO cyber_tasks
                    (title, description, is_completed, reminder_date, created_at)
                VALUES
                    (@title, @desc, @done, @reminder, @created);
                SELECT LAST_INSERT_ID();", con);

            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@desc", task.Description);
            cmd.Parameters.AddWithValue("@done", task.IsCompleted ? 1 : 0);
            cmd.Parameters.AddWithValue("@reminder", task.ReminderDate.HasValue
                                                       ? (object)task.ReminderDate.Value
                                                       : DBNull.Value);
            cmd.Parameters.AddWithValue("@created", task.CreatedAt);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            using var reader = new MySqlCommand(
                "SELECT * FROM cyber_tasks ORDER BY created_at DESC;", con)
                .ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? string.Empty : reader.GetString("description"),
                    IsCompleted = reader.GetInt32("is_completed") == 1,
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                                    ? null : reader.GetDateTime("reminder_date"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return tasks;
        }

        public void MarkCompleted(int id)
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var cmd = new MySqlCommand(
                "UPDATE cyber_tasks SET is_completed = 1 WHERE id = @id;", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int id)
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM cyber_tasks WHERE id = @id;", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}