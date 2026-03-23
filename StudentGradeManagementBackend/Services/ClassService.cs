using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class ClassService
    {
        private readonly string _connectionString;

        public ClassService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Class>> GetAllClassesAsync()
        {
            var classes = new List<Class>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"SELECT class_id, class_name, department_id 
                          FROM classes";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                classes.Add(new Class
                {
                    ClassId = reader.GetString(0),
                    ClassName = reader.GetString(1),
                    DepartmentId = reader.GetString(2)
                });
            }
            return classes;
        }

        public async Task AddClassAsync(Class classObj)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO classes (class_id, class_name, department_id) 
                          VALUES (@classId, @className, @departmentId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@classId", classObj.ClassId);
            command.Parameters.AddWithValue("@className", classObj.ClassName);
            command.Parameters.AddWithValue("@departmentId", classObj.DepartmentId);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateClassAsync(Class classObj)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"UPDATE classes 
                          SET class_name = @className, department_id = @departmentId 
                          WHERE class_id = @classId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@classId", classObj.ClassId);
            command.Parameters.AddWithValue("@className", classObj.ClassName);
            command.Parameters.AddWithValue("@departmentId", classObj.DepartmentId);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteClassAsync(string classId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM classes WHERE class_id = @classId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@classId", classId);
            await command.ExecuteNonQueryAsync();
        }
    }
}