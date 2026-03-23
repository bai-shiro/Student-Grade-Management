using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using StudentGradeManagementBackend.Models;
using StudentGradeManagementBackend.Services;

namespace StudentGradeManagementBackend.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnPostAsync(string UserId, string Password)
        {
            var role = await _userService.GetUserRoleAsync(UserId, Password);
            if (role != null)
            {
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("UserId", UserId);

                if (role == "admin")
                {
                    return RedirectToPage("/Index");
                }
                else if (role == "student")
                {
                    return RedirectToPage("/StudentDashboard");
                }
            }
            ViewData["Error"] = "’À∫≈ªÚ√‹¬Î¥ÌŒÛ";
            return Page();
        }
    }
}