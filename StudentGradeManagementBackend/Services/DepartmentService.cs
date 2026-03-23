using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class DepartmentService
    {
        private readonly string _connectionString;

        public DepartmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            var departments = new List<Department>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"SELECT department_id, department_name 
                          FROM departments";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                departments.Add(new Department
                {
                    DepartmentId = reader.GetString(0),
                    DepartmentName = reader.GetString(1)
                });
            }
            return departments;
        }

        public async Task AddDepartmentAsync(Department department)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO departments (department_id, department_name) 
                          VALUES (@departmentId, @departmentName)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@departmentId", department.DepartmentId);
            command.Parameters.AddWithValue("@departmentName", department.DepartmentName);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"UPDATE departments 
                          SET department_name = @departmentName 
                          WHERE department_id = @departmentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@departmentId", department.DepartmentId);
            command.Parameters.AddWithValue("@departmentName", department.DepartmentName);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteDepartmentAsync(string departmentId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM departments WHERE department_id = @departmentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@departmentId", departmentId);
            await command.ExecuteNonQueryAsync();
        }
    }
}