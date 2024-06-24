using System.ComponentModel.DataAnnotations.Schema;

namespace ReportMeeting.Models
{
    public class Task
    {
        public int id { get; set; }
        public string name { get; set; }
        public string? details { get; set; }
        public string? files { get; set; }
        public DateOnly? startDate { get; set; }
        public DateOnly? dueDate { get; set; }
        public int projectId { get; set; }
        public int assigneeId { get; set; }
        public string status { get; set; }
        public string? blockingPoint { get; set; }
        public Users assignee {  get; set; }
        [NotMapped]
        public IFormFile? file { get; set; }
    }
}
