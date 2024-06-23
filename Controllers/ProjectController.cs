using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    public class ProjectController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly TaskService _taskService;

        public ProjectController(AppDbContext context, TaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }

        // GET: Project
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 8)
        {
            // Calculate the total number of records
            var totalRecords = await _context.Project.CountAsync();

            // Calculate the total number of pages
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var user = HttpContext.Session.GetObject<Users>("User");

            var projects = new List<Project>();
            // if admin then show all list, else show only list according to user 
            if (user.roleId == 4)
            {
                // Fetch the paginated data
                projects = await _context.Project.Include(p => p.platform).Include(p => p.architect)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                // Fetch the paginated data
                projects = await _context.Project.Include(p => p.platform).Include(p => p.architect).Where(p => p.architectId == user.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Create a view model to pass the data and pagination details to the view
            var viewModel = new PaginationModel<Project>
            {
                Model = projects,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.Include(p => p.platform).Include(p => p.architect)
                .FirstOrDefaultAsync(m => m.id == id);

            var tasksList = await _taskService.getListByProject(id);

            if (project == null)
            {
                return NotFound();
            }

            ViewData["tasks"] = tasksList;
            return View(project);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            ViewData["architects"] = _context.Users.Where(a => a.role.name == "architect").ToList();
            ViewData["platforms"] = _context.Platform.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,details,startDate,dueDate,platformId,architectId,jiraLink")] Project project)
        {
            ModelState.Remove(nameof(project.platform));
            ModelState.Remove(nameof(project.architect));

            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,details,startDate,dueDate,platformId,architectId,jiraLink")] Project project)
        {
            if (id != project.id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(project.platform));
            ModelState.Remove(nameof(project.architect));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.Include(p => p.platform).Include(p => p.architect)
                .FirstOrDefaultAsync(m => m.id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.FindAsync(id);
            if (project != null)
            {
                _context.Project.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.id == id);
        }
    }
}
