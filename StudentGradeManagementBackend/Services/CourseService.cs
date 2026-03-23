// CourseService.cs 完整代码（无任何省略）
using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class CourseService
    {
        private readonly string _connectionString;

        // 构造函数
        public CourseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取所有课程
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            var courses = new List<Course>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT course_id, course_name, credit 
                          FROM courses 
                          ORDER BY course_id";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                courses.Add(new Course
                {
                    CourseId = reader.GetString(0).Trim(),
                    CourseName = reader.GetString(1).Trim(),
                    Credit = reader.GetDecimal(2)
                });
            }
            return courses;
        }

        // 根据课程ID获取课程
        public async Task<Course> GetCourseByIdAsync(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId)) return null;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT course_id, course_name, credit 
                          FROM courses 
                          WHERE course_id = @courseId LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@courseId", courseId.Trim());
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Course
                {
                    CourseId = reader.GetString(0).Trim(),
                    CourseName = reader.GetString(1).Trim(),
                    Credit = reader.GetDecimal(2)
                };
            }
            return null;
        }

        // 添加课程
        public async Task AddCourseAsync(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.CourseId)) throw new ArgumentException("课程编号不能为空");
            if (course.CourseId.Length > 10) throw new ArgumentException("课程编号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(course.CourseName)) throw new ArgumentException("课程名称不能为空");
            if (course.Credit <= 0 || course.Credit > 10) throw new ArgumentException("学分必须是0-10之间的有效数字");

            if (await GetCourseByIdAsync(course.CourseId) != null)
                throw new InvalidOperationException($"课程编号{course.CourseId}已存在，无法添加");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO courses (course_id, course_name, credit) 
                          VALUES (@courseId, @courseName, @credit)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@courseId", course.CourseId.Trim());
            command.Parameters.AddWithValue("@courseName", course.CourseName.Trim());
            command.Parameters.AddWithValue("@credit", course.Credit);
            
            await command.ExecuteNonQueryAsync();
        }

        // 更新课程
        public async Task UpdateCourseAsync(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.CourseId)) throw new ArgumentException("课程编号不能为空");
            if (course.CourseId.Length > 10) throw new ArgumentException("课程编号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(course.CourseName)) throw new ArgumentException("课程名称不能为空");
            if (course.Credit <= 0 || course.Credit > 10) throw new ArgumentException("学分必须是0-10之间的有效数字");

            if (await GetCourseByIdAsync(course.CourseId) == null)
                throw new KeyNotFoundException($"课程编号{course.CourseId}不存在，无法更新");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE courses 
                          SET course_name = @courseName, credit = @credit 
                          WHERE course_id = @courseId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@courseId", course.CourseId.Trim());
            command.Parameters.AddWithValue("@courseName", course.CourseName.Trim());
            command.Parameters.AddWithValue("@credit", course.Credit);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
                throw new InvalidOperationException($"更新课程失败：未匹配到编号{course.CourseId}的记录");
        }

        // 删除课程
        public async Task DeleteCourseAsync(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId)) throw new ArgumentException("课程编号不能为空");
            if (courseId.Length > 10) throw new ArgumentException("课程编号不能超过10个字符");

            if (await GetCourseByIdAsync(courseId) == null)
                throw new KeyNotFoundException($"课程编号{courseId}不存在，无法删除");

            // 检查关联成绩
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var checkQuery = "SELECT COUNT(*) FROM scores WHERE course_id = @courseId";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@courseId", courseId.Trim());
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
            if (count > 0)
                throw new InvalidOperationException($"课程{courseId}存在关联成绩，无法删除");

            // 执行删除
            var deleteQuery = "DELETE FROM courses WHERE course_id = @courseId";
            using var deleteCommand = new MySqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@courseId", courseId.Trim());
            await deleteCommand.ExecuteNonQueryAsync();
        }

        // 总学分计算方法
        public async Task<decimal> GetTotalCreditAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // 确保无数据时返回0，避免DBNull
            var query = "SELECT COALESCE(SUM(credit), 0) FROM courses";
            using var command = new MySqlCommand(query, connection);
            
            var result = await command.ExecuteScalarAsync();
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
    }
}