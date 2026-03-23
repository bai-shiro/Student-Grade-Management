using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentGradeManagementBackend.Models;
using StudentGradeManagementBackend.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentGradeManagementBackend.Pages
{
    public class StudentManagementModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly UserService _userService;
        private readonly ClassService _classService;
        private readonly DepartmentService _departmentService;

        // 构造函数注入所有依赖服务
        public StudentManagementModel(StudentService studentService, UserService userService, 
                                     ClassService classService, DepartmentService departmentService)
        {
            _studentService = studentService;
            _userService = userService;
            _classService = classService;
            _departmentService = departmentService;
            
            // 初始化所有列表，解决CS8618编译警告
            Students = new List<Student>();
            Classes = new List<Class>();
            Departments = new List<Department>();
        }

        // 页面展示数据
        public List<Student>? Students { get; set; }
        public List<Class>? Classes { get; set; }
        public List<Department>? Departments { get; set; }

        // 页面加载逻辑
        public async Task OnGet()
        {
            // 权限控制：仅管理员可访问
            var userRole = HttpContext.Session.GetString("Role");
            if (userRole != "admin")
            {
                Response.Redirect("/Login");
                return;
            }

            // 加载所有基础数据
            Students = await _studentService.GetAllStudentsAsync();
            Classes = await _classService.GetAllClassesAsync();
            Departments = await _departmentService.GetAllDepartmentsAsync();
        }

        // 处理所有POST请求（添加/编辑/删除）
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // 调试日志：打印所有请求参数
                Console.WriteLine("===== 学生管理POST参数 =====");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key} = {Request.Form[key]}");
                }

                // 1. 编辑学生
                if (Request.Form.ContainsKey("EditStudentId"))
                {
                    var studentId = Request.Form["EditStudentId"].ToString().Trim();
                    var name = Request.Form["EditName"].ToString().Trim();
                    var classId = Request.Form["EditClassId"].ToString().Trim();
                    var departmentId = Request.Form["EditDepartmentId"].ToString().Trim();
                    var enrollmentYearStr = Request.Form["EditEnrollmentYear"].ToString().Trim();

                    // 参数校验
                    ValidateStudentParams(studentId, name, classId, departmentId, enrollmentYearStr);
                    var enrollmentYear = int.Parse(enrollmentYearStr);

                    // 构建学生对象
                    var student = new Student
                    {
                        StudentId = studentId,
                        Name = name,
                        ClassId = classId,
                        DepartmentId = departmentId,
                        EnrollmentYear = enrollmentYear
                    };

                    // 执行更新
                    await _studentService.UpdateStudentAsync(student);
                    Console.WriteLine($"编辑学生成功：{studentId}");
                }
                // 2. 添加学生（关联创建用户）
                else if (Request.Form.ContainsKey("AddStudentId"))
                {
                    var studentId = Request.Form["AddStudentId"].ToString().Trim();
                    var name = Request.Form["AddName"].ToString().Trim();
                    var classId = Request.Form["AddClassId"].ToString().Trim();
                    var departmentId = Request.Form["AddDepartmentId"].ToString().Trim();
                    var enrollmentYearStr = Request.Form["AddEnrollmentYear"].ToString().Trim();
                    var initialPassword = Request.Form["AddInitialPassword"].ToString().Trim();

                    // 参数校验
                    ValidateStudentParams(studentId, name, classId, departmentId, enrollmentYearStr);
                    if (string.IsNullOrWhiteSpace(initialPassword))
                        throw new ArgumentException("初始密码不能为空");
                    var enrollmentYear = int.Parse(enrollmentYearStr);

                    // 检查学生/用户是否已存在
                    if (await _studentService.GetStudentByIdAsync(studentId) != null)
                        throw new InvalidOperationException($"学号{studentId}已存在，无法添加");
                    if (await _userService.GetUserByIdAsync(studentId) != null)
                        throw new InvalidOperationException($"学号{studentId}已作为用户编号存在，请更换学号");

                    // 1. 添加学生记录
                    var student = new Student
                    {
                        StudentId = studentId,
                        Name = name,
                        ClassId = classId,
                        DepartmentId = departmentId,
                        EnrollmentYear = enrollmentYear
                    };
                    await _studentService.AddStudentAsync(student);

                    // 2. 自动创建student角色用户
                    var studentUser = new User
                    {
                        UserId = studentId,
                        Password = initialPassword,
                        Role = "student"
                    };
                    await _userService.AddUserAsync(studentUser);
                    Console.WriteLine($"添加学生{studentId}成功，并创建关联用户");
                }
                // 3. 删除学生（关联删除用户）
                else if (Request.Form.ContainsKey("DeleteStudentId"))
                {
                    var studentId = Request.Form["DeleteStudentId"].ToString().Trim();
                    if (string.IsNullOrWhiteSpace(studentId))
                        throw new ArgumentException("学号不能为空");

                    // 检查学生是否存在
                    var existingStudent = await _studentService.GetStudentByIdAsync(studentId);
                    if (existingStudent == null)
                        throw new KeyNotFoundException($"学号{studentId}不存在，无法删除");

                    // 1. 删除学生记录
                    await _studentService.DeleteStudentAsync(studentId);

                    // 2. 删除关联的student角色用户
                    var existingUser = await _userService.GetUserByIdAsync(studentId);
                    if (existingUser != null && existingUser.Role == "student")
                    {
                        await _userService.DeleteUserAsync(studentId);
                        Console.WriteLine($"同步删除学生{studentId}的关联用户");
                    }
                }
                else
                {
                    return new BadRequestObjectResult(new { error = "无效的请求参数，未找到匹配的操作类型" });
                }

                return new OkResult();
            }
            catch (MySqlException ex)
            {
                // 数据库异常处理
                Console.WriteLine($"数据库异常：{ex.Number} - {ex.Message}");
                string errorMsg = ex.Number switch
                {
                    1062 => "学号/用户编号已存在",
                    1216 => "班级/院系不存在，违反外键约束",
                    1217 => "无法删除学生：存在关联的成绩记录",
                    _ => $"数据库错误：{ex.Message}"
                };
                return new ObjectResult(new { error = errorMsg })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception ex)
            {
                // 通用异常处理
                Console.WriteLine($"学生操作异常：{ex.Message}\n{ex.StackTrace}");
                return new ObjectResult(new { error = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 私有方法：学生参数通用校验
        private void ValidateStudentParams(string studentId, string name, string classId, 
                                           string departmentId, string enrollmentYearStr)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                throw new ArgumentException("学号不能为空");
            if (studentId.Length > 10)
                throw new ArgumentException("学号不能超过10个字符");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("姓名不能为空");
            if (string.IsNullOrWhiteSpace(classId))
                throw new ArgumentException("班级编号不能为空");
            if (string.IsNullOrWhiteSpace(departmentId))
                throw new ArgumentException("院系编号不能为空");
            if (!int.TryParse(enrollmentYearStr, out int enrollmentYear) || 
                enrollmentYear < 2000 || enrollmentYear > DateTime.Now.Year)
                throw new ArgumentException($"入学年份必须是2000-{DateTime.Now.Year}之间的有效数字");
        }
    }
}