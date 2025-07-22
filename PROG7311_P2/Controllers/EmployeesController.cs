using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using PROG7311_P2.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PROG7311_P2.Models;
using PROG7311_P2.Services;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PROG7311_P2.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IUserService _userService;
        private readonly FirebaseAuthProvider _auth;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IConfiguration _configuration;

        public EmployeesController(IUserService userService, ILogger<EmployeesController> logger, IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;

            // Initialize Firebase authentication provider
            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                throw new InvalidOperationException("Firebase API key is not configured");
            }
            _auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
        }

        public async Task<IActionResult> Home()
        {
            try
            {
                // Get the user id from the session
                var uid = HttpContext.Session.GetString("_UserToken");
                
                if (uid == null)
                {
                    // If the user is not logged in, redirect them to the login page
                    return RedirectToAction("Login", "Employees");
                }

                // Check if the user is an employee in the database
                var employee = await _userService.GetEmployeeByIdAsync(uid);
                if (employee != null)
                {
                    // User is logged in and is an employee, show the home page
                    return View();
                }

                // If the user is not an employee, redirect them to the appropriate home page
                var farmer = await _userService.GetFarmerByIdAsync(uid);
                if (farmer != null)
                {
                    return RedirectToAction("Home", "Farmers");
                }

                // Unknown user, redirect to login
                return RedirectToAction("Login", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Employees Home action");
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password")] LoginRegisterModel employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create a new instance of the validation methods class
                    ValidationMethods validation = new ValidationMethods();

                    // Check if the email already exists and if they do, show error message
                    if (await _userService.EmployeeExistsAsync(employee.Email))
                    {
                        ModelState.AddModelError("Email", "User with this email already exists.");
                    }
                    // Check if the password meets the requirements and if it doesn't, show error message
                    else if (!validation.PasswordRequirements(employee.Password))
                    {
                        ModelState.AddModelError("Password", "Password must contain at least 8 characters, 1 uppercase, 1 lowercase, 1 number and 1 special character.");
                    }
                    // Check if the email is valid and if it isn't, show error message
                    else if (!validation.IsEmailValid(employee.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be valid.");
                    }
                    // Check if the name only contains letters and if it doesn't, show error message
                    else if (!validation.OnlyLetters(employee.Name))
                    {
                        ModelState.AddModelError("Name", "Name must only contain letters.");
                    }
                    // Check if the email is in the organization and if it isn't, show error message
                    else if (!validation.IsEmailInOrganization(employee.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be in the organization.");
                    }
                    else
                    {
                        // Create a new user in firebase authentication
                        var fbAuthLink = await _auth.CreateUserWithEmailAndPasswordAsync(employee.Email, employee.Password);

                        // Get the user id and auth token from firebase authentication
                        var uid = fbAuthLink.User.LocalId;
                        var authToken = fbAuthLink.FirebaseToken;
                      
                        if (uid != null)
                        {
                            // Create a new employee object excluding the password field
                            Employee newEmployee = new Employee
                            {
                                Id = uid,
                                Name = employee.Name,
                                Email = employee.Email,
                                UserRoleId = (int) Roles.Employee
                            };

                            // Add the new employee to the database
                            await _userService.AddEmployeeAsync(newEmployee);

                            // Set the session data to log them in immediately
                            HttpContext.Session.SetString("_UserToken", uid);
                            HttpContext.Session.SetString("_UserEmail", employee.Email);
                            HttpContext.Session.SetString("_AuthToken", authToken);
                            HttpContext.Session.SetInt32("_UserRole", (int) Roles.Employee);
                            
                            _logger.LogInformation("Employee created and logged in: {Email}", employee.Email);
                            return RedirectToAction("Home", "Employees");
                        }
                    }
                }
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Code Attribution
                // Title: How to Integrate Firebase in ASP NET Core MVC
                // https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.Error.Message);
                _logger.LogError(ex, "Firebase authentication error during employee creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee: {EmployeeName}", employee.Name);
                ModelState.AddModelError("", "An error occurred while creating the employee. Please try again.");
            }

            return View(employee);
        }

        // Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginRegisterModel user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the employee from the database
                    var employee = await _userService.GetEmployeeByEmailAsync(user.Email);

                    // Get the farmer from the database
                    var farmer = await _userService.GetFarmerByEmailAsync(user.Email);

                    // If the employee exists, log them in
                    if (employee != null)
                    {
                        // Code Attribution
                        // Title: How to Integrate Firebase in ASP NET Core MVC
                        // https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application

                        // Log in an existing user
                        var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
                        var uid = fbAuthLink.User.LocalId;
                        var authToken = fbAuthLink.FirebaseToken; // Get the authentication token

                        if (uid != null)
                        {
                            // Set the user id to the logged in user
                            HttpContext.Session.SetString("_UserToken", uid);
                            HttpContext.Session.SetString("_UserEmail", user.Email);
                            HttpContext.Session.SetString("_AuthToken", authToken); // Store the auth token
                            
                            // Set the user role to the logged in user
                            HttpContext.Session.SetInt32("_UserRole", (int) Roles.Employee);
                            
                            _logger.LogInformation("Employee logged in successfully: {Email}", user.Email);
                            return RedirectToAction("Home", "Employees");
                        }
                    }
                    // If the farmer exists, log them in
                    else if (farmer != null)
                    {
                        // Use the same client-side authentication as employees
                        var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
                        var uid = fbAuthLink.User.LocalId;
                        var authToken = fbAuthLink.FirebaseToken; // Get the authentication token

                        if (uid != null)
                        {
                            // Set the user id to the logged in user
                            HttpContext.Session.SetString("_UserToken", uid);
                            HttpContext.Session.SetString("_UserEmail", user.Email);
                            HttpContext.Session.SetString("_AuthToken", authToken); // Store the auth token
                            
                            // Set the user role to the logged in user
                            HttpContext.Session.SetInt32("_UserRole", (int) Roles.Farmer);
                            
                            _logger.LogInformation("Farmer logged in successfully: {Email}", user.Email);
                            return RedirectToAction("Home", "Farmers");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Invalid email or password.");
                    }
                }
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Code Attribution
                // Title: How to Integrate Firebase in ASP NET Core MVC
                // https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.Error.Message);
                _logger.LogError(ex, "Firebase authentication error during login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", user.Email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }
    }
}
