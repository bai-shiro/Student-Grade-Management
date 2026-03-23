using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentGradeManagementBackend.Services;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly CourseService _courseService;

        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
        // 新增TotalCredit属性
        public decimal TotalCredit { get; set; } 

        public IndexModel(StudentService studentService, CourseService courseService)
        {
            _studentService = studentService;
            _courseService = courseService;
        }

        public async Task OnGet()
        {
            try
            {
                StudentCount = (await _studentService.GetAllStudentsAsync()).Count;
                CourseCount = (await _courseService.GetAllCoursesAsync()).Count;
                TotalCredit = await _courseService.GetTotalCreditAsync();
            }
            catch (System.Exception ex)
            {
                // 记录日志，这里简单输出到控制台，实际应用中应使用日志框架
                System.Console.WriteLine($"获取数据出错: {ex.Message}");
                // 可以传递错误信息到视图
                ViewData["Error"] = "获取数据时出现错误，请稍后重试";
            }
        }
    }
}