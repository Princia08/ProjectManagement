using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportMeeting.Data;
using ReportMeeting.Models;
using ReportMeeting.Services;

namespace ReportMeeting.Controllers
{
    public class TaskController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly TaskService _taskService;

        public TaskController(AppDbContext context, TaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }

        // GET: Task
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Task.Include(t => t.assignee);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task
                .Include(t => t.assignee)
                .FirstOrDefaultAsync(m => m.id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Task/Create
        public async Task<IActionResult> Create(int projectId)
        {
            var users = await _context.Users.Where(u => u.roleId != 3 && u.roleId != 4).ToListAsync();
            ViewData["assigneeId"] = new SelectList(users, "id", "name");
            ViewData["projectId"] = projectId;

            var project = await _context.Project.FindAsync(projectId);
            ViewData["projectName"] = project.name;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,details,files,startDate,dueDate,projectId,assigneeId,status,blockingPoint")] Models.Task task)
        {
            ModelState.Remove(nameof(task.assignee));

            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), "Project", new { id = task.projectId });
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            var users = await _context.Users.Where(u => u.roleId != 3 && u.roleId != 4).ToListAsync();
            ViewData["assignee"] = new SelectList(users, "id", "name");
            return View(task);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,details,file,files,startDate,dueDate,projectId,assigneeId,status,blockingPoint")] Models.Task task)
        {
            ModelState.Remove(nameof(task.assignee));

            if (ModelState.IsValid)
            {
                // Handle file upload
                if (task.file != null && task.file.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", task.file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await task.file.CopyToAsync(stream);
                    }
                    task.files = task.file.FileName;
                }

                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
            }        

            return RedirectToAction(nameof(Details), "Project", new { id = task.projectId });
        }

        // GET: Task/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task
                .Include(t => t.assignee)
                .FirstOrDefaultAsync(m => m.id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Task.FindAsync(id);
            if (task != null)
            {
                _context.Task.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Task.Any(e => e.id == id);
        }

        [HttpGet]
        public async Task<ActionResult<List<Models.Task>>> List(int? projectId)
        {
            var project = await _context.Project.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project doesn't exist");
            }
            var tasksList = await _taskService.getListByProject(projectId);
            return Ok(tasksList);
        }
    }
}
