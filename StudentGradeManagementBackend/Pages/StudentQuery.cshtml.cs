using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace StudentGradeManagementBackend.Pages
{
    public class ScoreViewModel
    {
        public required string CourseName { get; set; }
        public decimal Grade { get; set; }
    }

    public class StudentQueryModel : PageModel
    {
        private readonly string _connectionString;

        public StudentQueryModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(_connectionString), "数据库连接字符串为空");
        }

        public required List<ScoreViewModel> MyScores { get; set; }

        public async Task OnGetAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            MyScores = new List<ScoreViewModel>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"SELECT c.CourseName, s.Grade
                          FROM Students st
                          JOIN Scores s ON st.Id = s.StudentId
                          JOIN Courses c ON s.CourseId = c.CourseId
                          WHERE st.Name = @Username";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                MyScores.Add(new ScoreViewModel
                {
                    CourseName = reader.GetString(0),
                    Grade = reader.GetDecimal(1)
                });
            }
        }
    }
}