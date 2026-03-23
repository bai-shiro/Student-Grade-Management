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
    public class UserManagementModel : PageModel
    {
        private readonly UserService _userService;

        public UserManagementModel(UserService userService)
        {
            _userService = userService;
            Users = new List<User>();
        }

        public List<User>? Users { get; set; }

        public async Task OnGet()
        {
            Users = await _userService.GetAllUsersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // 调试日志
                Console.WriteLine("===== 用户管理POST参数 =====");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key} = {Request.Form[key]}");
                }

                // 1. 编辑用户
                if (Request.Form.ContainsKey("EditUserId"))
                {
                    var userId = Request.Form["EditUserId"].ToString().Trim();
                    var password = Request.Form["EditPassword"].ToString().Trim();
                    var role = Request.Form["EditRole"].ToString().Trim();

                    // 修复：角色校验改为admin/student，匹配数据库
                    if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("用户编号不能为空");
                    if (userId.Length > 10) throw new ArgumentException("用户编号不能超过10个字符");
                    if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("密码不能为空");
                    if (string.IsNullOrWhiteSpace(role) || (role != "admin" && role != "student")) 
                        throw new ArgumentException("角色必须是admin或student");

                    var user = new User
                    {
                        UserId = userId,
                        Password = password,
                        Role = role
                    };
                    await _userService.UpdateUserAsync(user);
                }
                // 2. 添加用户
                else if (Request.Form.ContainsKey("AddUserId"))
                {
                    var userId = Request.Form["AddUserId"].ToString().Trim();
                    var password = Request.Form["AddPassword"].ToString().Trim();
                    var role = Request.Form["AddRole"].ToString().Trim();

                    if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("用户编号不能为空");
                    if (userId.Length > 10) throw new ArgumentException("用户编号不能超过10个字符");
                    if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("密码不能为空");
                    if (string.IsNullOrWhiteSpace(role) || (role != "admin" && role != "student")) 
                        throw new ArgumentException("角色必须是admin或student");

                    var existingUser = await _userService.GetUserByIdAsync(userId);
                    if (existingUser != null) throw new InvalidOperationException($"用户编号{userId}已存在，无法添加");

                    var user = new User
                    {
                        UserId = userId,
                        Password = password,
                        Role = role
                    };
                    await _userService.AddUserAsync(user);
                }
                // 3. 删除用户
                else if (Request.Form.ContainsKey("DeleteUserId"))
                {
                    var userId = Request.Form["DeleteUserId"].ToString().Trim();
                    if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("用户编号不能为空");

                    var existingUser = await _userService.GetUserByIdAsync(userId);
                    if (existingUser == null) throw new KeyNotFoundException($"用户编号{userId}不存在，无法删除");

                    await _userService.DeleteUserAsync(userId);
                }
                else
                {
                    return new BadRequestObjectResult(new { error = "无效的请求参数：未找到匹配的操作类型" });
                }

                return new OkResult();
            }
            // 修复：改为MySql.Data.MySqlClient.MySqlException
            catch (MySqlException ex)
            {
                Console.WriteLine($"数据库异常：{ex.Message} (Code: {ex.Number})");
                string errorMsg = ex.Number switch
                {
                    1265 => "角色值无效（仅允许admin/student）",
                    1062 => "用户编号已存在",
                    _ => $"数据库错误：{ex.Message}"
                };
                return new ObjectResult(new { error = errorMsg })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作异常：{ex.Message}\n{ex.StackTrace}");
                return new ObjectResult(new { error = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}