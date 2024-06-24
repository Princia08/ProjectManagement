namespace ReportMeeting.Models
{
    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public DateTime startDate { get; set; }
        public DateTime dueDate { get; set; }
        public int platformId { get; set; }
        public int architectId { get; set; }
        public string jiraLink { get; set; }
        public Platform platform { get; set; }
        public Users architect {  get; set; }
    }
}
