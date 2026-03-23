using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取用户角色（登录验证）
        public async Task<string> GetUserRoleAsync(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("用户编号不能为空");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("密码不能为空");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT role FROM users WHERE user_id = @userId AND password = @password";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId.Trim());
            command.Parameters.AddWithValue("@password", password.Trim());
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString()?.Trim();
        }

        // 获取所有用户
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT user_id, password, role 
                          FROM users 
                          ORDER BY user_id";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetString(0).Trim(),
                    Password = reader.GetString(1).Trim(),
                    Role = reader.GetString(2).Trim()
                });
            }
            return users;
        }

        // 根据ID获取用户
        public async Task<User> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT user_id, password, role 
                          FROM users 
                          WHERE user_id = @userId LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId.Trim());
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetString(0).Trim(),
                    Password = reader.GetString(1).Trim(),
                    Role = reader.GetString(2).Trim()
                };
            }
            return null;
        }

        // 添加用户
        public async Task AddUserAsync(User user)
        {
            // 修复：角色校验匹配数据库ENUM
            if (string.IsNullOrWhiteSpace(user.UserId)) throw new ArgumentException("用户编号不能为空");
            if (user.UserId.Length > 10) throw new ArgumentException("用户编号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentException("密码不能为空");
            if (string.IsNullOrWhiteSpace(user.Role) || (user.Role != "admin" && user.Role != "student")) 
                throw new ArgumentException("角色必须是admin或student");

            if (await GetUserByIdAsync(user.UserId) != null) 
                throw new InvalidOperationException($"用户编号{user.UserId}已存在，无法添加");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO users (user_id, password, role) 
                          VALUES (@userId, @password, @role)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", user.UserId.Trim());
            command.Parameters.AddWithValue("@password", user.Password.Trim());
            command.Parameters.AddWithValue("@role", user.Role.Trim());
            
            await command.ExecuteNonQueryAsync();
        }

        // 更新用户
        public async Task UpdateUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserId)) throw new ArgumentException("用户编号不能为空");
            if (user.UserId.Length > 10) throw new ArgumentException("用户编号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentException("密码不能为空");
            if (string.IsNullOrWhiteSpace(user.Role) || (user.Role != "admin" && user.Role != "student")) 
                throw new ArgumentException("角色必须是admin或student");

            if (await GetUserByIdAsync(user.UserId) == null) 
                throw new KeyNotFoundException($"用户编号{user.UserId}不存在，无法更新");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE users 
                          SET password = @password, role = @role 
                          WHERE user_id = @userId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", user.UserId.Trim());
            command.Parameters.AddWithValue("@password", user.Password.Trim());
            command.Parameters.AddWithValue("@role", user.Role.Trim());
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0) 
                throw new InvalidOperationException($"更新用户失败：未匹配到用户编号{user.UserId}的记录");
        }

        // 删除用户
        public async Task DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("用户编号不能为空");
            if (userId.Length > 10) throw new ArgumentException("用户编号不能超过10个字符");

            if (await GetUserByIdAsync(userId) == null) 
                throw new KeyNotFoundException($"用户编号{userId}不存在，无法删除");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "DELETE FROM users WHERE user_id = @userId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId.Trim());
            
            await command.ExecuteNonQueryAsync();
        }
    }
}