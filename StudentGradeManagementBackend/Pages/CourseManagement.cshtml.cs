using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentGradeManagementBackend.Models;
using StudentGradeManagementBackend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Pages
{
    public class CourseManagementModel : PageModel
    {
        private readonly CourseService _courseService;

        public CourseManagementModel(CourseService courseService)
        {
            _courseService = courseService;
            Courses = new List<Course>();
        }

        public List<Course>? Courses { get; set; }

        public async Task OnGet()
        {
            Courses = await _courseService.GetAllCoursesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // 调试：强制打印所有参数（包括空值）
                Console.WriteLine("======= POST请求参数 =======");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key} = [{Request.Form[key]}]"); // 加[]显式看空值
                }

                // ========== 核心修复：简化判断逻辑，只检查Key存在 ==========
                // 1. 处理编辑课程（最高优先级）
                if (Request.Form.ContainsKey("EditCourseId") && Request.Form.ContainsKey("EditCourseName") && Request.Form.ContainsKey("EditCredit"))
                {
                    // 手动提取参数（避免空值拦截）
                    var courseId = Request.Form["EditCourseId"].ToString().Trim();
                    var courseName = Request.Form["EditCourseName"].ToString().Trim();
                    var creditStr = Request.Form["EditCredit"].ToString().Trim();

                    // 单独校验参数（不依赖外层判断）
                    if (string.IsNullOrWhiteSpace(courseId)) throw new ArgumentException("课程编号不能为空");
                    if (string.IsNullOrWhiteSpace(courseName)) throw new ArgumentException("课程名称不能为空");
                    if (!decimal.TryParse(creditStr, out decimal credit) || credit < 0) throw new ArgumentException("学分必须是≥0的数字");

                    // 适配required修饰符：强制初始化所有required字段
                    var course = new Course
                    {
                        CourseId = courseId,
                        CourseName = courseName,
                        Credit = credit
                    };
                    await _courseService.UpdateCourseAsync(course);
                    Console.WriteLine($"编辑成功：{courseId} - {courseName}");
                }
                // 2. 处理添加课程
                else if (Request.Form.ContainsKey("CourseId") && Request.Form.ContainsKey("CourseName") && Request.Form.ContainsKey("Credit"))
                {
                    var courseId = Request.Form["CourseId"].ToString().Trim();
                    var courseName = Request.Form["CourseName"].ToString().Trim();
                    var creditStr = Request.Form["Credit"].ToString().Trim();

                    if (string.IsNullOrWhiteSpace(courseId)) throw new ArgumentException("课程编号不能为空");
                    if (string.IsNullOrWhiteSpace(courseName)) throw new ArgumentException("课程名称不能为空");
                    if (!decimal.TryParse(creditStr, out decimal credit) || credit < 0) throw new ArgumentException("学分必须是≥0的数字");

                    var course = new Course
                    {
                        CourseId = courseId,
                        CourseName = courseName,
                        Credit = credit
                    };
                    await _courseService.AddCourseAsync(course);
                    Console.WriteLine($"添加成功：{courseId} - {courseName}");
                }
                // 3. 处理删除课程
                else if (Request.Form.ContainsKey("DeleteCourseId"))
                {
                    var courseId = Request.Form["DeleteCourseId"].ToString().Trim();
                    if (string.IsNullOrWhiteSpace(courseId)) throw new ArgumentException("课程编号不能为空");

                    await _courseService.DeleteCourseAsync(courseId);
                    Console.WriteLine($"删除成功：{courseId}");
                }
                // 4. 无效参数
                else
                {
                    return new BadRequestObjectResult(new { error = "无效的请求参数：未找到匹配的操作类型" });
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"请求处理失败：{ex.Message}\n{ex.StackTrace}");
                return new ObjectResult(new { error = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<IActionResult> OnGetGetCourseByIdAsync(string CourseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CourseId))
                {
                    return new JsonResult(new { success = false, message = "课程ID不能为空" });
                }

                var course = await _courseService.GetCourseByIdAsync(CourseId);
                if (course == null)
                {
                    return new JsonResult(new { success = false, message = $"未找到课程ID：{CourseId}" });
                }

                return new JsonResult(new
                {
                    success = true,
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Credit = course.Credit
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}