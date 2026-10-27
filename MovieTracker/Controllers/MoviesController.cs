
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Data;
using MovieTracker.Models;
using MovieTracker.Services;

namespace MovieTracker.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _settings;

        public MoviesController(
            ApplicationDbContext context,
            AppSettings settings)
        {
            _context = context;
            _settings = settings;
        }

        // SELECT: показва филмите и позволява филтриране.
        public async Task<IActionResult> Index(
            string? searchTitle,
            string? genre,
            int? year)
        {
            var movies = _context.Movies.AsNoTracking().AsQueryable();

            // Филтър по заглавие.
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                movies = movies.Where(m =>
                    EF.Functions.Like(m.Title, $"%{searchTitle}%"));
            }

            // Филтър по жанр.
            if (!string.IsNullOrWhiteSpace(genre))
            {
                movies = movies.Where(m => m.Genre == genre);
            }

            // Филтър по година.
            if (year.HasValue)
            {
                movies = movies.Where(m =>
                    m.ReleaseYear == year.Value);
            }

            // Данните за филмите се извличат от SQLite.
            var result = await movies
                .OrderBy(m => m.Title)
                .ToListAsync();

            // Данни за филтъра по жанр.
            ViewBag.Genres = await _context.Movies
                .AsNoTracking()
                .Select(m => m.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            // Запазваме избраните филтри във View.
            ViewBag.SearchTitle = searchTitle;
            ViewBag.SelectedGenre = genre;
            ViewBag.Year = year;

            // Демонстрираме използването на Singleton.
            ViewBag.ApplicationName = _settings.ApplicationName;
            ViewBag.Version = _settings.Version;

            return View(result);
        }

        // GET: показва празната форма за добавяне.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: приема формата и добавя филма в БД.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Намираме филма в базата данни по неговото ID.
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            // Изпращаме намерения филм към View.
            return View(movie);
        }

        // POST: Movies/Edit/5
        // Запазва промените по съществуващия филм.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            // Проверяваме дали ID в URL съвпада с ID на модела.
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            // Обновяваме съществуващия запис.
            _context.Movies.Update(movie);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Намираме филма, който потребителят иска да изтрие.
            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        // Изтрива филма след потвърждение.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
