namespace StudentGradeManagementBackend.Models
{
    public class User
    {
        public required string UserId { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
}