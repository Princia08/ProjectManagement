namespace ReportMeeting.Models
{
    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly dueDate { get; set; }
        public string jiraLink { get; set; }
        public string platformConcerned { get; set; }
        public int architect_id { get; set; }
    }
}
