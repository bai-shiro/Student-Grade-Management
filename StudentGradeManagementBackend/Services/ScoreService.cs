using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class ScoreService
    {
        private readonly string _connectionString;

        public ScoreService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取所有成绩
        public async Task<List<Score>> GetAllScoresAsync()
        {
            var scores = new List<Score>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT id, student_id, course_id, score 
                          FROM scores 
                          ORDER BY id";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                scores.Add(new Score
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1).Trim(),
                    CourseId = reader.GetString(2).Trim(),
                    ScoreValue = reader.GetDecimal(3)
                });
            }
            return scores;
        }

        // 根据学生学号获取成绩
        public async Task<List<Score>> GetScoresByStudentIdAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return new List<Score>();

            var scores = new List<Score>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT id, student_id, course_id, score 
                          FROM scores 
                          WHERE student_id = @studentId 
                          ORDER BY id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", studentId.Trim());
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                scores.Add(new Score
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1).Trim(),
                    CourseId = reader.GetString(2).Trim(),
                    ScoreValue = reader.GetDecimal(3)
                });
            }
            return scores;
        }

        // 根据成绩ID获取成绩
        public async Task<Score> GetScoreByIdAsync(int scoreId)
        {
            if (scoreId <= 0) return null;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT id, student_id, course_id, score 
                          FROM scores 
                          WHERE id = @scoreId LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@scoreId", scoreId);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Score
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1).Trim(),
                    CourseId = reader.GetString(2).Trim(),
                    ScoreValue = reader.GetDecimal(3)
                };
            }
            return null;
        }

        // 根据学生和课程获取成绩（检查重复）
        public async Task<Score> GetScoreByStudentAndCourseAsync(string studentId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId)) return null;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT id, student_id, course_id, score 
                          FROM scores 
                          WHERE student_id = @studentId AND course_id = @courseId LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", studentId.Trim());
            command.Parameters.AddWithValue("@courseId", courseId.Trim());
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Score
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1).Trim(),
                    CourseId = reader.GetString(2).Trim(),
                    ScoreValue = reader.GetDecimal(3)
                };
            }
            return null;
        }

        // 添加成绩
        public async Task AddScoreAsync(Score score)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(score.StudentId)) throw new ArgumentException("学生学号不能为空");
            if (string.IsNullOrWhiteSpace(score.CourseId)) throw new ArgumentException("课程编号不能为空");
            if (score.ScoreValue < 0 || score.ScoreValue > 100) throw new ArgumentException("成绩必须是0-100之间的有效数字");

            // 检查是否已存在该学生的该课程成绩
            if (await GetScoreByStudentAndCourseAsync(score.StudentId, score.CourseId) != null)
                throw new InvalidOperationException($"学号{score.StudentId}的{score.CourseId}课程成绩已存在，无法重复添加");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO scores (student_id, course_id, score) 
                          VALUES (@studentId, @courseId, @score)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", score.StudentId.Trim());
            command.Parameters.AddWithValue("@courseId", score.CourseId.Trim());
            command.Parameters.AddWithValue("@score", score.ScoreValue);
            
            await command.ExecuteNonQueryAsync();
        }

        // 更新成绩（仅更新成绩值，核心修复）
        public async Task UpdateScoreAsync(Score score)
        {
            // 参数校验
            if (score.Id <= 0) throw new ArgumentException("成绩ID无效");
            if (score.ScoreValue < 0 || score.ScoreValue > 100) throw new ArgumentException("成绩必须是0-100之间的有效数字");

            // 检查成绩是否存在
            var existingScore = await GetScoreByIdAsync(score.Id);
            if (existingScore == null)
                throw new KeyNotFoundException($"成绩ID{score.Id}不存在，无法更新");

            // 仅更新成绩值，不修改学生/课程ID
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE scores 
                          SET score = @score 
                          WHERE id = @scoreId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@scoreId", score.Id);
            command.Parameters.AddWithValue("@score", score.ScoreValue);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
                throw new InvalidOperationException($"更新成绩失败：未匹配到ID{score.Id}的记录");
        }

        // 删除成绩
        public async Task DeleteScoreAsync(int scoreId)
        {
            // 参数校验
            if (scoreId <= 0) throw new ArgumentException("成绩ID无效");

            // 检查成绩是否存在
            var existingScore = await GetScoreByIdAsync(scoreId);
            if (existingScore == null)
                throw new KeyNotFoundException($"成绩ID{scoreId}不存在，无法删除");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "DELETE FROM scores WHERE id = @scoreId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@scoreId", scoreId);
            
            await command.ExecuteNonQueryAsync();
        }
    }
}