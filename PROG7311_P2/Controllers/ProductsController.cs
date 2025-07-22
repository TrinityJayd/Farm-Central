using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PROG7311_P2.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Models;
using PROG7311_P2.Services;

namespace PROG7311_P2.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, IUserService userService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _userService = userService;
            _logger = logger;
        }

        private async Task PopulateViewBagAsync()
        {
            try
            {
                // Get authentication token from session
                var authToken = HttpContext.Session.GetString("_AuthToken");

                // Get product types for the dropdown
                var productTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                ViewBag.TypeNames = productTypes.Select(pt => pt.Text).ToList();

                // Get unique farmer emails for the dropdown (for employees only)
                var products = await _productService.GetAllProductsForEmployeeAsync(authToken);
                var farmerEmails = products.Select(p => p.Email).Distinct().ToList();
                ViewBag.Farmers = farmerEmails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating ViewBag data");
                // Set empty lists to prevent null reference exceptions
                ViewBag.TypeNames = new List<string>();
                ViewBag.Farmers = new List<string>();
            }
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                string uid = HttpContext.Session.GetString("_UserToken");

                if (uid == null)
                {
                    _logger.LogWarning("User not logged in, redirecting to login");
                    return RedirectToAction("Login", "Employees");
                }

                var authToken = HttpContext.Session.GetString("_AuthToken");

                // Populate ViewBag data for dropdowns
                await PopulateViewBagAsync();

                // Check if the user is an employee
                var employee = await _userService.GetEmployeeByIdAsync(uid);
                if (employee != null)
                {
                    var products = await _productService.GetAllProductsForEmployeeAsync(authToken);
                    return View(products);
                }

                // Check if the user is a farmer
                var farmer = await _userService.GetFarmerByIdAsync(uid);
                if (farmer != null)
                {
                    var products = await _productService.GetAllProductsForFarmerAsync(farmer.Email, authToken);
                    return View(products);
                }

                // User not found in either collection
                _logger.LogWarning("User not found in database: {UserId}", uid);
                return RedirectToAction("Login", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Products Index action");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                string uid = HttpContext.Session.GetString("_UserToken");

                if (uid == null)
                {
                    _logger.LogWarning("User not logged in, redirecting to login");
                    return RedirectToAction("Login", "Employees");
                }

                // Check if the user is a farmer in the database
                var farmer = await _userService.GetFarmerByIdAsync(uid);
                if (farmer == null)
                {
                    _logger.LogWarning("Non-farmer user attempted to access product creation: {UserId}", uid);
                    return RedirectToAction("Home", "Employees");
                }

                var product = new Product
                {
                    Email = farmer.Email
                };

                var authToken = HttpContext.Session.GetString("_AuthToken");
                ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Products Create GET action");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Quantity,Price,DateSupplied,Email")] Product product, string typeName)
        {
            // Get auth token once at the beginning
            var authToken = HttpContext.Session.GetString("_AuthToken");
            
            try
            {
                _logger.LogInformation("Starting product creation for: {ProductName} by {Email}", product.ProductName, product.Email);
                
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(authToken))
                    {
                        _logger.LogError("Authentication token is null or empty");
                        ModelState.AddModelError("", "Authentication failed. Please log in again.");
                        ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                        return View(product);
                    }

                    _logger.LogInformation("Creating product with type: {TypeName}", typeName);
                    var createdProduct = await _productService.CreateProductAsync(product, typeName, authToken);
                    _logger.LogInformation("Product created successfully: {ProductName} by {FarmerEmail}", product.ProductName, product.Email);
                    return RedirectToAction("Home", "Farmers");
                }
                else
                {
                    _logger.LogWarning("Model state is invalid. Errors: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }

                ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                return View(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product validation failed: {ProductName}", product.ProductName);
                ModelState.AddModelError("", ex.Message);
                ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                return View(product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in product creation: {ProductName}", product.ProductName);
                ModelState.AddModelError("", ex.Message);
                ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating product: {ProductName} by {Email}", product.ProductName, product.Email);
                ModelState.AddModelError("", "An unexpected error occurred while creating the product. Please try again.");
                ViewBag.ProductTypes = await _productService.GetProductTypeSelectListAsync(authToken);
                return View(product);
            }
        }

        // POST: Products/Filter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filter(string farmer, string startDate, string endDate, string typeName)
        {
            try
            {
                string uid = HttpContext.Session.GetString("_UserToken");
                if (uid == null)
                {
                    _logger.LogWarning("User not logged in, redirecting to login");
                    return RedirectToAction("Login", "Employees");
                }

                // Check if the user is an employee or farmer
                var employee = await _userService.GetEmployeeByIdAsync(uid);
                var farmerUser = await _userService.GetFarmerByIdAsync(uid);
                
                int role;
                string? currentUserEmail = null;

                if (employee != null)
                {
                    role = (int)Roles.Employee;
                }
                else if (farmerUser != null)
                {
                    role = (int)Roles.Farmer;
                    currentUserEmail = farmerUser.Email;
                }
                else
                {
                    _logger.LogWarning("User not found in database: {UserId}", uid);
                    return RedirectToAction("Login", "Employees");
                }

                // Check if all filters are default values
                if ((farmer == "All" || farmer == null) && startDate == null && endDate == null && typeName == "All")
                {
                    return RedirectToAction("Index");
                }

                // Populate ViewBag data for dropdowns
                await PopulateViewBagAsync();

                var authToken = HttpContext.Session.GetString("_AuthToken");
                var products = await _productService.FilterProductsAsync(farmer, startDate, endDate, typeName, role, currentUserEmail, authToken);
                
                _logger.LogInformation("Products filtered successfully. Filters: Farmer={Farmer}, StartDate={StartDate}, EndDate={EndDate}, Type={TypeName}", 
                    farmer, startDate, endDate, typeName);

                return View("Index", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}

