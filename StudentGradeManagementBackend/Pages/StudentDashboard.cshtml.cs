using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using StudentGradeManagementBackend.Models;
using StudentGradeManagementBackend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Pages
{
    public class StudentDashboardModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly ScoreService _scoreService;
        private readonly CourseService _courseService;

        // 修复：初始化时显式设置所有required属性（避免CS9035）
        public Student Student { get; set; } = new Student
        {
            StudentId = string.Empty,
            Name = string.Empty,
            ClassId = string.Empty,
            DepartmentId = string.Empty
        };
        
        public List<StudentScoreWithCourse> Scores { get; set; } = new List<StudentScoreWithCourse>();

        public StudentDashboardModel(StudentService studentService, ScoreService scoreService, CourseService courseService)
        {
            _studentService = studentService;
            _scoreService = scoreService;
            _courseService = courseService;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // 1. 校验Session（学生ID/角色）
                var studentId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("Role");
                
                if (string.IsNullOrEmpty(studentId) || userRole != "student")
                {
                    return RedirectToPage("/Login");
                }

                // 2. 获取学生信息（强化空值校验）
                var student = await _studentService.GetStudentByIdAsync(studentId);
                if (student == null)
                {
                    // 清除无效Session
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login");
                }
                // 赋值（避免直接初始化空对象）
                Student = student;

                // 3. 获取学生成绩并关联课程信息
                var studentScores = await _scoreService.GetScoresByStudentIdAsync(studentId);
                if (studentScores == null || studentScores.Count == 0)
                {
                    return Page(); // 无成绩直接返回
                }

                // 批量获取课程（优化性能）
                var courseIds = studentScores.Select(s => s.CourseId).Distinct().ToList();
                var allCourses = new Dictionary<string, Course>();
                foreach (var courseId in courseIds)
                {
                    var course = await _courseService.GetCourseByIdAsync(courseId);
                    if (course != null)
                    {
                        allCourses[courseId] = course;
                    }
                }

                // 组装成绩+课程数据（修复Course必需属性初始化）
                Scores = studentScores
                    .Where(s => allCourses.ContainsKey(s.CourseId))
                    .Select(s => new StudentScoreWithCourse
                    {
                        ScoreValue = s.ScoreValue,
                        // 直接使用数据库查询的Course对象（已包含所有required属性）
                        Course = allCourses[s.CourseId]
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"学生面板加载异常：{ex.Message}\n{ex.StackTrace}");
                return RedirectToPage("/Login");
            }

            return Page();
        }
    }

    // 数据模型：成绩+课程关联（修复Course初始化）
    public class StudentScoreWithCourse
    {
        public decimal ScoreValue { get; set; }
        // 修复：初始化时设置所有required属性
        public Course Course { get; set; } = new Course
        {
            CourseId = string.Empty,
            CourseName = string.Empty,
            Credit = 0
        };
    }
}

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.AspNetCore.Http;
// using StudentGradeManagementBackend.Models;
// using StudentGradeManagementBackend.Services;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace StudentGradeManagementBackend.Pages
// {
//     public class StudentDashboardModel : PageModel
//     {
//         private readonly StudentService _studentService;
//         private readonly ScoreService _scoreService;
//         private readonly CourseService _courseService;

//         public Student Student { get; set; }
//         public List<StudentScoreWithCourse> Scores { get; set; }

//         public StudentDashboardModel(StudentService studentService, ScoreService scoreService, CourseService courseService)
//         {
//             _studentService = studentService;
//             _scoreService = scoreService;
//             _courseService = courseService;
//         }

//         public async Task<IActionResult> OnGet()
//         {
//             var studentId = HttpContext.Session.GetString("UserId");
//             if (string.IsNullOrEmpty(studentId))
//             {
//                 return RedirectToPage("/Login");
//             }

//             Student = await _studentService.GetStudentByIdAsync(studentId);
//             if (Student == null)
//             {
//                 return RedirectToPage("/Login");
//             }

//             var studentScores = await _scoreService.GetScoresByStudentIdAsync(studentId);
//             Scores = new List<StudentScoreWithCourse>();

//             foreach (var score in studentScores)
//             {
//                 var course = await _courseService.GetCourseByIdAsync(score.CourseId);
//                 if (course!= null)
//                 {
//                     Scores.Add(new StudentScoreWithCourse
//                     {
//                         ScoreValue = score.ScoreValue,
//                         Course = course
//                     });
//                 }
//             }

//             return Page();
//         }
//     }

//     public class StudentScoreWithCourse
//     {
//         public decimal ScoreValue { get; set; }
//         public Course Course { get; set; }
//     }
// }