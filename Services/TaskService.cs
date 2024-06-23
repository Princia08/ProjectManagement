using Microsoft.EntityFrameworkCore;
using ReportMeeting.Data;
using ReportMeeting.Models;

namespace ReportMeeting.Services
{
    public class TaskService
    {
        private readonly AppDbContext _context;
        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Task>> getListByProject(int? projectId)
        {
            var tasks = await _context.Task.Where(t => t.projectId == projectId ).Include(t => t.assignee).ToListAsync();
            return tasks;
        }
    }
}
