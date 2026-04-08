using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;

namespace Rodoslovnaya
{
    public class PersonRepository
    {
        private readonly string _connectionString;

        public PersonRepository()
        {
            // Создаем путь к базе данных в специальной папке приложений
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificPath = Path.Combine(appDataPath, "FamilyTreeApp");
            
            // Создаем папку, если она не существует
            if (!Directory.Exists(appSpecificPath))
            {
                Directory.CreateDirectory(appSpecificPath);
            }
            
            string dbPath = Path.Combine(appSpecificPath, "family_tree.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            
            // Создаем таблицу, если она не существует
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Persons (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    BirthYear INTEGER,
                    Parent1Id INTEGER,
                    Parent2Id INTEGER,
                    FOREIGN KEY (Parent1Id) REFERENCES Persons(Id),
                    FOREIGN KEY (Parent2Id) REFERENCES Persons(Id)
                )";

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public void AddPerson(Person person)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "INSERT INTO Persons (Name, BirthYear, Parent1Id, Parent2Id) VALUES (@Name, @BirthYear, @Parent1Id, @Parent2Id)";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", person.Name);
            command.Parameters.AddWithValue("@BirthYear", person.BirthYear.HasValue ? (object)person.BirthYear.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Parent1Id", person.Parent1Id.HasValue ? (object)person.Parent1Id.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Parent2Id", person.Parent2Id.HasValue ? (object)person.Parent2Id.Value : DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public List<Person> GetAllPersons()
        {
            var persons = new List<Person>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name, BirthYear, Parent1Id, Parent2Id FROM Persons ORDER BY Name";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var person = new Person
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString(),
                    BirthYear = reader["BirthYear"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["BirthYear"]),
                    Parent1Id = reader["Parent1Id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Parent1Id"]),
                    Parent2Id = reader["Parent2Id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Parent2Id"])
                };

                persons.Add(person);
            }

            return persons;
        }

        public Person GetPersonById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name, BirthYear, Parent1Id, Parent2Id FROM Persons WHERE Id = @Id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Person
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString(),
                    BirthYear = reader["BirthYear"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["BirthYear"]),
                    Parent1Id = reader["Parent1Id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Parent1Id"]),
                    Parent2Id = reader["Parent2Id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Parent2Id"])
                };
            }

            return null;
        }

        public void UpdatePerson(Person person)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "UPDATE Persons SET Name = @Name, BirthYear = @BirthYear, Parent1Id = @Parent1Id, Parent2Id = @Parent2Id WHERE Id = @Id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", person.Id);
            command.Parameters.AddWithValue("@Name", person.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BirthYear", person.BirthYear.HasValue ? (object)person.BirthYear.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Parent1Id", person.Parent1Id.HasValue ? (object)person.Parent1Id.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Parent2Id", person.Parent2Id.HasValue ? (object)person.Parent2Id.Value : DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public void DeletePerson(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM Persons WHERE Id = @Id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            command.ExecuteNonQuery();
        }
    }
}