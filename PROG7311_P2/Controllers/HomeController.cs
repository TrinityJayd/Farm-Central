using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Models;
using PROG7311_P2.Services;
using System.Diagnostics;

namespace PROG7311_P2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFirebaseDatabaseService _firebaseService;

        public HomeController(ILogger<HomeController> logger, IFirebaseDatabaseService firebaseService)
        {
            _logger = logger;
            _firebaseService = firebaseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        // Test Firebase connection
        public async Task<IActionResult> TestFirebase()
        {
            try
            {
                // Test basic connection by trying to get product types
                var productTypes = await _firebaseService.GetAllProductTypesAsync();
                return Json(new { success = true, message = $"Firebase connected! Found {productTypes.Count()} product types." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firebase connection test failed");
                return Json(new { 
                    success = false, 
                    message = $"Firebase connection failed: {ex.Message}",
                    details = ex.ToString(),
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // Test Firebase database setup
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test if we can write to the database
                var testData = new { test = "connection", timestamp = DateTime.Now };
                
                // This will help us see if the database exists and is accessible
                return Json(new { 
                    success = true, 
                    message = "Database test endpoint ready",
                    config = new {
                        databaseUrl = _firebaseService.GetType().GetField("_firebaseClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_firebaseService)?.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database test failed");
                return Json(new { 
                    success = false, 
                    message = $"Database test failed: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }
    }
}