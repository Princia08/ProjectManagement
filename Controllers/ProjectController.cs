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
using Rotativa.AspNetCore;

namespace ReportMeeting.Controllers
{
    public class ProjectController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly TaskService _taskService;
        private readonly ProjectService _projectService;
        
        public ProjectController(AppDbContext context, TaskService taskService, IConfiguration configuration)
        {
            _context = context;
            _taskService = taskService;
            string connectionString = configuration.GetConnectionString("ReportingAppContext");
            _projectService = new ProjectService(connectionString);
        }

        // GET: Project
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 8)
        {
            var user = HttpContext.Session.GetObject<Users>("User");
            // Call the ProjectService to get projects
            var viewModel = await _projectService.GetProjectsAsync(pageNumber, pageSize, user);

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

            var viewModel = new ProjectDetailsModel
            {
                Project = project,
                Tasks = tasksList
            };
            
            return View(viewModel);
        }

        public async Task<IActionResult> ExportPdf(int id)
        {
            var project = await _context.Project.Include(p => p.platform).Include(p => p.architect)
                .FirstOrDefaultAsync(m => m.id == id);

            var tasksList = await _taskService.getListByProject(id);

            var viewModel = new ProjectDetailsModel
            {
                Project = project,
                Tasks = tasksList
            };

            return new ViewAsPdf("Details", viewModel)
            {
                FileName = "ProjectDetails.pdf"
            };
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

            ViewData["architects"] = _context.Users.Where(a => a.role.name == "architect").ToList();
            ViewData["platforms"] = _context.Platform.ToList();
            
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
