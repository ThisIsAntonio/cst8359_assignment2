using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Assignment2.Data;
using Assignment2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Assignment2.Controllers
{
    public class NewsController : Controller
    {
        private readonly SportsDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<NewsController> _logger;

        public NewsController(SportsDbContext context, BlobServiceClient blobServiceClient, ILogger<NewsController> logger)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        // GET: News
        public async Task<IActionResult> Index(string sportClubId)
        {
            _logger.LogInformation("News Index called with sportClubId: {SportClubId}", sportClubId);

            if (string.IsNullOrEmpty(sportClubId))
            {
                _logger.LogWarning("SportClubId is null or empty");
                return BadRequest("SportClubId is required");
            }

            ViewData["SportClubId"] = sportClubId;
            _logger.LogInformation("SportClubId set to ViewData: {SportClubId}", sportClubId);

            var sportsDbContext = _context.News.Include(n => n.SportClub).Where(n => n.SportClubId == sportClubId);

            return View(await sportsDbContext.ToListAsync());
        }


        // GET: News/Create
        public IActionResult Create(string sportClubId)
        {
            _logger.LogInformation("====> Creating a News for SportClubId: {SportClubId}", sportClubId);

            if (string.IsNullOrEmpty(sportClubId))
            {
                return BadRequest("SportClubId is required.");
            }

            _logger.LogInformation("Creating News for SportClubId: {SportClubId}", sportClubId);
            var news = new News { SportClubId = sportClubId };
            return View(news);
        }



        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NewsId,FileName,Url,SportClubId")] News news, IFormFile uploadFile)
        {
            _logger.LogInformation("Received request to create news for SportClubId: {SportClubId}", news.SportClubId);

            if (uploadFile != null)
            {
                try
                {
                    var containerName = $"{news.SportClubId.ToLower()}-images"; // Ensure container name is lowercase and add suffix
                    if (containerName.Length > 63) // Limit container name length
                    {
                        containerName = containerName.Substring(0, 63);
                    }

                    var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    await blobContainerClient.CreateIfNotExistsAsync();

                    // Generate a random filename
                    var randomFileName = GenerateRandomFileName(10, 15) + ".jpg";
                    var blobClient = blobContainerClient.GetBlobClient(randomFileName);
                    _logger.LogInformation("Uploading file {FileName} to blob container {ContainerName}", randomFileName, containerName);

                    using (var stream = uploadFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    news.Url = blobClient.Uri.ToString();
                    news.FileName = randomFileName;
                    _logger.LogInformation("File uploaded successfully: {FileName}, URL: {Url}", news.FileName, news.Url);
                }
                catch (Azure.RequestFailedException ex)
                {
                    _logger.LogError(ex, "Error uploading file to Azure Blob Storage");
                    ModelState.AddModelError(string.Empty, "Error uploading file. Please try again.");
                    ViewData["FileUploadError"] = "Error uploading file. Please try again.";
                }
            }
            else
            {
                _logger.LogWarning("No file uploaded for news creation.");
                ModelState.AddModelError(string.Empty, "Please upload a file.");
                ViewData["FileUploadError"] = "Please upload a file.";
            }

            // Load the corresponding SportClub from the database and assign it to news
            if (!string.IsNullOrEmpty(news.SportClubId))
            {
                _logger.LogInformation("Attempting to find SportClub with id {SportClubId}", news.SportClubId);
                var sportClub = await _context.SportClubs.FindAsync(news.SportClubId);
                if (sportClub != null)
                {
                    news.SportClub = sportClub;
                    _logger.LogInformation("SportClub with id {SportClubId} found and assigned: {SportClub}", news.SportClubId, sportClub);
                }
                else
                {
                    _logger.LogWarning("SportClub with id {SportClubId} not found", news.SportClubId);
                    ModelState.AddModelError(string.Empty, "Invalid SportClubId");
                }
            }
            else
            {
                _logger.LogWarning("SportClubId is null or empty in the request");
                ModelState.AddModelError(string.Empty, "SportClubId is required.");
            }

            // Print all properties of news for debugging
            _logger.LogInformation("News properties: NewsId={NewsId}, FileName={FileName}, Url={Url}, SportClubId={SportClubId}, SportClub={SportClub}",
                news.NewsId, news.FileName, news.Url, news.SportClubId, news.SportClub);

            // Force model validation
            ModelState.Clear();
            TryValidateModel(news);

            // Validate and print model state errors for debugging
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    _logger.LogInformation("ModelState {Key}: {Value}", state.Key, state.Value.Errors.Select(e => e.ErrorMessage));
                }
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewData["SportClubId"] = new SelectList(_context.SportClubs, "Id", "Id", news.SportClubId);
                return View(news);
            }

            _context.Add(news);
            await _context.SaveChangesAsync();
            _logger.LogInformation("News created successfully for SportClubId: {SportClubId}", news.SportClubId);
            return RedirectToAction(nameof(Index), new { sportClubId = news.SportClubId });
        }

        private string GenerateRandomFileName(int minLength, int maxLength)
        {
            var random = new Random();
            var length = random.Next(minLength, maxLength);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var fileName = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            return fileName;
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete request received with null id.");
                return NotFound();
            }

            var news = await _context.News
                .Include(n => n.SportClub)
                .FirstOrDefaultAsync(m => m.NewsId == id);
            if (news == null)
            {
                _logger.LogWarning("News with id {Id} not found for deletion.", id);
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sportClubId)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                _logger.LogInformation("News with id {Id} removed.", id);
            }
            else
            {
                _logger.LogWarning("News with id {Id} not found for deletion.", id);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { sportClubId });
        }



        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.NewsId == id);
        }
    }
}
