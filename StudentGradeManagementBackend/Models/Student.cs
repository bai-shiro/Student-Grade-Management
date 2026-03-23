namespace StudentGradeManagementBackend.Models
{
    public class Student
    {
        public required string StudentId { get; set; }
        public required string Name { get; set; }
        public required string ClassId { get; set; }
        public required string DepartmentId { get; set; }
        public int EnrollmentYear { get; set; }
    }
}