namespace StudentGradeManagementBackend.Models
{
    public class Course
    {
        public required string CourseId { get; set; }
        public required string CourseName { get; set; }
        public decimal Credit { get; set; }
    }
}