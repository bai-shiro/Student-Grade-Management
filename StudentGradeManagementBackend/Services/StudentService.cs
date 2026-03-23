using MySql.Data.MySqlClient;
using StudentGradeManagementBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Services
{
    public class StudentService
    {
        private readonly string _connectionString;

        public StudentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取所有学生
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var students = new List<Student>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT student_id, name, class_id, department_id, enrollment_year 
                          FROM students 
                          ORDER BY student_id";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    StudentId = reader.GetString(0).Trim(),
                    Name = reader.GetString(1).Trim(),
                    ClassId = reader.GetString(2).Trim(),
                    DepartmentId = reader.GetString(3).Trim(),
                    EnrollmentYear = reader.GetInt32(4)
                });
            }
            return students;
        }

        // 根据学号获取学生
        public async Task<Student> GetStudentByIdAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return null;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT student_id, name, class_id, department_id, enrollment_year 
                          FROM students 
                          WHERE student_id = @studentId LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", studentId.Trim());
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Student
                {
                    StudentId = reader.GetString(0).Trim(),
                    Name = reader.GetString(1).Trim(),
                    ClassId = reader.GetString(2).Trim(),
                    DepartmentId = reader.GetString(3).Trim(),
                    EnrollmentYear = reader.GetInt32(4)
                };
            }
            return null;
        }

        // 添加学生
        public async Task AddStudentAsync(Student student)
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(student.StudentId)) throw new ArgumentException("学号不能为空");
            if (student.StudentId.Length > 10) throw new ArgumentException("学号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(student.Name)) throw new ArgumentException("姓名不能为空");
            if (string.IsNullOrWhiteSpace(student.ClassId)) throw new ArgumentException("班级编号不能为空");
            if (string.IsNullOrWhiteSpace(student.DepartmentId)) throw new ArgumentException("院系编号不能为空");
            if (student.EnrollmentYear < 2000 || student.EnrollmentYear > DateTime.Now.Year) 
                throw new ArgumentException($"入学年份必须是2000-{DateTime.Now.Year}之间的有效数字");

            // 检查学生是否已存在
            if (await GetStudentByIdAsync(student.StudentId) != null) 
                throw new InvalidOperationException($"学号{student.StudentId}已存在，无法添加");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO students (student_id, name, class_id, department_id, enrollment_year) 
                          VALUES (@studentId, @name, @classId, @departmentId, @enrollmentYear)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", student.StudentId.Trim());
            command.Parameters.AddWithValue("@name", student.Name.Trim());
            command.Parameters.AddWithValue("@classId", student.ClassId.Trim());
            command.Parameters.AddWithValue("@departmentId", student.DepartmentId.Trim());
            command.Parameters.AddWithValue("@enrollmentYear", student.EnrollmentYear);
            
            await command.ExecuteNonQueryAsync();
        }

        // 更新学生
        public async Task UpdateStudentAsync(Student student)
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(student.StudentId)) throw new ArgumentException("学号不能为空");
            if (student.StudentId.Length > 10) throw new ArgumentException("学号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(student.Name)) throw new ArgumentException("姓名不能为空");
            if (string.IsNullOrWhiteSpace(student.ClassId)) throw new ArgumentException("班级编号不能为空");
            if (string.IsNullOrWhiteSpace(student.DepartmentId)) throw new ArgumentException("院系编号不能为空");
            if (student.EnrollmentYear < 2000 || student.EnrollmentYear > DateTime.Now.Year) 
                throw new ArgumentException($"入学年份必须是2000-{DateTime.Now.Year}之间的有效数字");

            // 检查学生是否存在
            if (await GetStudentByIdAsync(student.StudentId) == null) 
                throw new KeyNotFoundException($"学号{student.StudentId}不存在，无法更新");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE students 
                          SET name = @name, class_id = @classId, department_id = @departmentId, enrollment_year = @enrollmentYear 
                          WHERE student_id = @studentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", student.StudentId.Trim());
            command.Parameters.AddWithValue("@name", student.Name.Trim());
            command.Parameters.AddWithValue("@classId", student.ClassId.Trim());
            command.Parameters.AddWithValue("@departmentId", student.DepartmentId.Trim());
            command.Parameters.AddWithValue("@enrollmentYear", student.EnrollmentYear);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0) 
                throw new InvalidOperationException($"更新学生失败：未匹配到学号{student.StudentId}的记录");
        }

        // 删除学生
        public async Task DeleteStudentAsync(string studentId)
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(studentId)) throw new ArgumentException("学号不能为空");
            if (studentId.Length > 10) throw new ArgumentException("学号不能超过10个字符");

            // 检查学生是否存在
            if (await GetStudentByIdAsync(studentId) == null) 
                throw new KeyNotFoundException($"学号{studentId}不存在，无法删除");

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "DELETE FROM students WHERE student_id = @studentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@studentId", studentId.Trim());
            
            await command.ExecuteNonQueryAsync();
        }
    }
}