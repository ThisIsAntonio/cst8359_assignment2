using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Models.ViewModels;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Assignment2.Controllers
{
    public class SportClubsController : Controller
    {
        private readonly SportsDbContext _context;
        private readonly ILogger<SportClubsController> _logger;

        public SportClubsController(SportsDbContext context, ILogger<SportClubsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SportClubs
        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            var viewModel = new NewsViewModel
            {
                SportClubs = await _context.SportClubs.ToListAsync(),
                Fans = await _context.Fans.ToListAsync(),
                Subscriptions = await _context.Subscriptions.Include(s => s.Fan).Include(s => s.SportClub).ToListAsync()
            };

            if (!string.IsNullOrEmpty(id))
            {
                var selectedFans = viewModel.Subscriptions
                    .Where(s => s.SportClubId == id)
                    .Select(s => s.Fan)
                    .ToList();
                ViewData["SelectedFans"] = selectedFans;
            }

            return View(viewModel);
        }

        // GET: SportClubs/News/5
        [HttpGet]
        public Task<IActionResult> News(string id)
        {
            _logger.LogInformation("Redirecting to News Index with sportClubId: {SportClubId}", id);
            return Task.FromResult<IActionResult>(RedirectToAction("Index", "News", new { sportClubId = id }));
        }

        // GET: SportClubs/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportClub = await _context.SportClubs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sportClub == null)
            {
                return NotFound();
            }

            return View(sportClub);
        }

        // GET: SportClubs/Select/5
        [HttpGet]
        public async Task<IActionResult> Select(string id)
        {
            var sportClub = await _context.SportClubs
                .Include(sc => sc.Subscriptions)
                .ThenInclude(s => s.Fan)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sportClub == null)
            {
                return NotFound();
            }

            var selectedFans = sportClub.Subscriptions.Select(s => s.Fan).ToList();
            ViewData["SelectedFans"] = selectedFans;

            var viewModel = new NewsViewModel
            {
                SportClubs = await _context.SportClubs.ToListAsync(),
                Fans = await _context.Fans.ToListAsync(),
                Subscriptions = await _context.Subscriptions.ToListAsync()
            };

            return View("Index", viewModel);
        }


        // GET: SportClubs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SportClubs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Fee")] SportClub sportClub)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sportClub);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sportClub);
        }

        // GET: SportClubs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportClub = await _context.SportClubs.FindAsync(id);
            if (sportClub == null)
            {
                return NotFound();
            }
            return View(sportClub);
        }

        // POST: SportClubs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Fee")] SportClub sportClub)
        {
            if (id != sportClub.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sportClub);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SportClubExists(sportClub.Id))
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
            return View(sportClub);
        }

        // GET: SportClubs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sportClub = await _context.SportClubs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sportClub == null)
            {
                return NotFound();
            }

            return View(sportClub);
        }

        // POST: SportClubs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sportClub = await _context.SportClubs.FindAsync(id);
            if (sportClub != null)
            {
                _context.SportClubs.Remove(sportClub);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SportClubExists(string id)
        {
            return _context.SportClubs.Any(e => e.Id == id);
        }

    }
}
