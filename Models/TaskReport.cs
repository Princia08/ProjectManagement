namespace ReportMeeting.Models
{
    public class TaskReport
    {
        public int id { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly dueDate { get; set; }
        public string blockingPoint { get; set; }
        public string platform { get; set; }
        public int status_id { get; set; }
        public int developper_id { get; set; }
        public int project_id { get; set; }
        public int type_id { get; set; }
    }
}
