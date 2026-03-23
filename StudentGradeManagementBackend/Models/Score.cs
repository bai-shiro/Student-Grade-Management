namespace StudentGradeManagementBackend.Models
{
    public class Score
    {
        public int Id { get; set; }
        public required string StudentId { get; set; }
        public required string CourseId { get; set; }
        public decimal ScoreValue { get; set; }
    }
}