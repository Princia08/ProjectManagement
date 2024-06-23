using System.ComponentModel.DataAnnotations.Schema;

namespace ReportMeeting.Models
{
    public class Users
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string phoneNumber { get; set; }

        [ForeignKey("roleId")]
        public int roleId { get; set; }
        
        public Role role { get; set; }
    }
}
