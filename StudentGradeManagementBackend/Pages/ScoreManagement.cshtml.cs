using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentGradeManagementBackend.Models;
using StudentGradeManagementBackend.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Pages
{
    public class ScoreManagementModel : PageModel
    {
        private readonly ScoreService _scoreService;
        private readonly StudentService _studentService;
        private readonly CourseService _courseService;

        // 构造函数注入依赖服务
        public ScoreManagementModel(ScoreService scoreService, StudentService studentService, CourseService courseService)
        {
            _scoreService = scoreService;
            _studentService = studentService;
            _courseService = courseService;
            
            // 初始化列表，解决CS8618编译警告
            Scores = new List<Score>();
            Students = new List<Student>();
            Courses = new List<Course>();
        }

        // 页面展示数据
        public List<Score>? Scores { get; set; }
        public List<Student>? Students { get; set; }
        public List<Course>? Courses { get; set; }
        public string? CurrentUserId { get; set; } // 当前登录用户ID
        public string? CurrentUserRole { get; set; } // 当前登录用户角色

        // 页面加载逻辑
        public async Task OnGet()
        {
            // 获取当前登录用户信息
            CurrentUserId = HttpContext.Session.GetString("UserId");
            CurrentUserRole = HttpContext.Session.GetString("Role");

            // 未登录用户跳转登录页
            if (string.IsNullOrWhiteSpace(CurrentUserId) || string.IsNullOrWhiteSpace(CurrentUserRole))
            {
                Response.Redirect("/Login");
                return;
            }

            // 加载基础数据（学生/课程）
            Students = await _studentService.GetAllStudentsAsync();
            Courses = await _courseService.GetAllCoursesAsync();

            // 权限控制：admin查看所有成绩，student仅查看自己的成绩
            if (CurrentUserRole == "admin")
            {
                Scores = await _scoreService.GetAllScoresAsync();
            }
            else if (CurrentUserRole == "student")
            {
                Scores = await _scoreService.GetScoresByStudentIdAsync(CurrentUserId);
            }
        }

        // 处理所有POST请求（添加/编辑/删除）
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // 获取当前用户信息
                CurrentUserId = HttpContext.Session.GetString("UserId");
                CurrentUserRole = HttpContext.Session.GetString("Role");

                // 权限校验：仅admin可执行添加/编辑/删除操作
                if (CurrentUserRole != "admin")
                {
                    return new ForbidResult("仅管理员可操作成绩数据");
                }

                // 调试日志：打印所有请求参数
                Console.WriteLine("===== 成绩管理POST参数 =====");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key} = {Request.Form[key]}");
                }

                // 1. 编辑成绩（核心修复：仅校验ID和成绩值）
                if (Request.Form.ContainsKey("EditScoreId"))
                {
                    var scoreIdStr = Request.Form["EditScoreId"].ToString().Trim();
                    var scoreValueStr = Request.Form["EditScoreValue"].ToString().Trim();

                    // 基础校验
                    if (!int.TryParse(scoreIdStr, out int scoreId) || scoreId <= 0)
                        throw new ArgumentException($"成绩ID无效：{scoreIdStr}");
                    ValidateScoreValue(scoreValueStr);
                    var scoreValue = decimal.Parse(scoreValueStr);

                    // 从数据库获取原始成绩数据（避免依赖前端传学生/课程ID）
                    var existingScore = await _scoreService.GetScoreByIdAsync(scoreId);
                    if (existingScore == null)
                        throw new KeyNotFoundException($"成绩ID{scoreId}不存在，无法编辑");

                    // 仅更新成绩值
                    existingScore.ScoreValue = scoreValue;
                    await _scoreService.UpdateScoreAsync(existingScore);
                    Console.WriteLine($"编辑成绩成功：ID={scoreId}，新成绩={scoreValue}");
                }
                // 2. 添加成绩（保留原有逻辑）
                else if (Request.Form.ContainsKey("AddStudentId"))
                {
                    var studentId = Request.Form["AddStudentId"].ToString().Trim();
                    var courseId = Request.Form["AddCourseId"].ToString().Trim();
                    var scoreValueStr = Request.Form["AddScoreValue"].ToString().Trim();

                    // 参数校验
                    ValidateScoreParams(studentId, courseId, scoreValueStr);
                    var scoreValue = decimal.Parse(scoreValueStr);

                    // 检查学生/课程是否存在
                    if (await _studentService.GetStudentByIdAsync(studentId) == null)
                        throw new KeyNotFoundException($"学号{studentId}不存在");
                    if (await _courseService.GetCourseByIdAsync(courseId) == null)
                        throw new KeyNotFoundException($"课程编号{courseId}不存在");

                    // 检查是否已存在该学生的该课程成绩
                    var existingScore = await _scoreService.GetScoreByStudentAndCourseAsync(studentId, courseId);
                    if (existingScore != null)
                        throw new InvalidOperationException($"学号{studentId}的{courseId}课程成绩已存在，无法重复添加");

                    // 构建成绩对象
                    var score = new Score
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        ScoreValue = scoreValue
                    };

                    // 执行添加
                    await _scoreService.AddScoreAsync(score);
                    Console.WriteLine($"添加成绩成功：学生{studentId} - 课程{courseId}");
                }
                // 3. 删除成绩（保留原有逻辑）
                else if (Request.Form.ContainsKey("DeleteScoreId"))
                {
                    var scoreIdStr = Request.Form["DeleteScoreId"].ToString().Trim();
                    if (!int.TryParse(scoreIdStr, out int scoreId) || scoreId <= 0)
                        throw new ArgumentException("成绩ID无效");

                    // 检查成绩是否存在
                    var existingScore = await _scoreService.GetScoreByIdAsync(scoreId);
                    if (existingScore == null)
                        throw new KeyNotFoundException($"成绩ID{scoreId}不存在，无法删除");

                    // 执行删除
                    await _scoreService.DeleteScoreAsync(scoreId);
                    Console.WriteLine($"删除成绩成功：ID={scoreId}");
                }
                else
                {
                    return new BadRequestObjectResult("无效的请求参数，未找到匹配的操作类型");
                }

                return new OkResult();
            }
            catch (MySqlException ex)
            {
                // 数据库异常处理
                Console.WriteLine($"数据库异常：{ex.Number} - {ex.Message}");
                string errorMsg = ex.Number switch
                {
                    1216 => "学生/课程不存在，违反外键约束",
                    _ => $"数据库错误：{ex.Message}"
                };
                return new ObjectResult(errorMsg)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception ex)
            {
                // 通用异常处理
                Console.WriteLine($"成绩操作异常：{ex.Message}\n{ex.StackTrace}");
                return new ObjectResult(ex.Message)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 私有方法：添加成绩专用校验（学生+课程+成绩值）
        private void ValidateScoreParams(string studentId, string courseId, string scoreValueStr)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                throw new ArgumentException("学生学号不能为空");
            if (string.IsNullOrWhiteSpace(courseId))
                throw new ArgumentException("课程编号不能为空");
            ValidateScoreValue(scoreValueStr);
        }

        // 私有方法：编辑成绩专用校验（仅成绩值）
        private void ValidateScoreValue(string scoreValueStr)
        {
            if (!decimal.TryParse(scoreValueStr, out decimal scoreValue) || scoreValue < 0 || scoreValue > 100)
                throw new ArgumentException("成绩必须是0-100之间的有效数字");
        }
    }
}