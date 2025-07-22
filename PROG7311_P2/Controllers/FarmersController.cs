using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using PROG7311_P2.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311_P2.Models;
using PROG7311_P2.Services;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PROG7311_P2.Controllers
{
    public class FarmersController : Controller
    {
        private readonly IUserService _userService;
        private readonly FirebaseAdmin.Auth.FirebaseAuth _auth;
        private readonly FirebaseAuthProvider _authUser;
        private readonly ILogger<FarmersController> _logger;
        private readonly IConfiguration _configuration;

        public FarmersController(IUserService userService, ILogger<FarmersController> logger, IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
            _auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            
            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                throw new InvalidOperationException("Firebase API key is not configured");
            }
            _authUser = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
        }

        public async Task<IActionResult> Home()
        {
            try
            {
                // Get uid
                var uid = HttpContext.Session.GetString("_UserToken");

                // Check if the user is logged in
                if (uid != null)
                {
                    // Check in the database if the user is a farmer
                    var farmer = await _userService.GetFarmerByIdAsync(uid);

                    // If the user is a farmer
                    if (farmer != null)
                    {
                        _logger.LogInformation("Farmer found in database - Farmer Role: {FarmerRole}", farmer.UserRoleId);
                        return View();
                    }
                    else
                    {
                        _logger.LogWarning("Farmer not found in database for user ID: {UserId}", uid);
                    }

                    // If the user is not a farmer
                    return RedirectToAction("Home", "Employees");
                }

                // If the user is not logged in
                return RedirectToAction("Login", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Farmers Home action");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Farmers/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // ONLY EMPLOYEES CAN CREATE FARMERS

                // Get the uid
                var uid = HttpContext.Session.GetString("_UserToken");
                
                if (uid != null)
                {
                    // Check in the database if the user is an employee
                    var employee = await _userService.GetEmployeeByIdAsync(uid);
                    if (employee != null)
                    {
                        return View();
                    }
                    // If the user is not an employee
                    return RedirectToAction("Home", "Farmers");
                }
                // If the user is not logged in
                return RedirectToAction("Login", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Farmers Create GET action");
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Farmers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Phone,Email")] Farmer farmer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create a new instance of the validation methods class
                    ValidationMethods validation = new ValidationMethods();

                    // Check if the email is taken
                    if (await _userService.FarmerExistsAsync(farmer.Email))
                    {
                        ModelState.AddModelError("Email", "Email is already taken.");
                    }
                    // Check if the email is valid
                    else if (!validation.IsEmailValid(farmer.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be valid.");
                    }
                    // Check if the phone number is valid
                    else if (!validation.IsPhoneNumberValid(farmer.Phone))
                    {
                        ModelState.AddModelError("Phone", "Phone number must be 10 digits long.");
                    }
                    // Check if the name is valid
                    else if (!validation.OnlyLetters(farmer.Name))
                    {
                        ModelState.AddModelError("Name", "Name must only contain letters.");
                    }
                    // Check if the email is in the organization
                    else if (!validation.IsEmailInOrganization(farmer.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be in the organization.");
                    }
                    else
                    {
                        // For now the password will be set to Password1* until the user logs in and changes it
                        string password = "Password1*";

                        // Create a new user using client-side authentication (same as employee creation)
                        var fbAuthLink = await _authUser.CreateUserWithEmailAndPasswordAsync(farmer.Email, password);

                        // Get the uid of the created user
                        var userUid = fbAuthLink.User.LocalId;

                        if (userUid != null)
                        {
                            // Create a new farmer
                            Farmer newFarmer = new()
                            {
                                Id = userUid,
                                Name = farmer.Name,
                                Address = farmer.Address,
                                Phone = farmer.Phone,
                                Email = farmer.Email,
                                UserRoleId = (int) Roles.Farmer,
                            };

                            // Add the farmer to the database
                            await _userService.AddFarmerAsync(newFarmer);

                            _logger.LogInformation("Farmer created successfully: {FarmerName} with ID {FarmerId}", farmer.Name, userUid);
                            return RedirectToAction("Home", "Employees");
                        }
                    }              
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating farmer: {FarmerName}", farmer.Name);
                ModelState.AddModelError("", "An error occurred while creating the farmer. Please try again.");
            }
            
            return View(farmer);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            try
            {
                // Get uid
                var uid = HttpContext.Session.GetString("_UserToken");
                if (uid != null)
                {
                    // Check if the user is a farmer
                    var farmer = await _userService.GetFarmerByIdAsync(uid);
                    if (farmer != null)
                    {
                        return View();
                    }
                    return RedirectToAction("Home", "Employees");
                }
                return RedirectToAction("Login", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePassword GET action");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(LoginRegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create a new instance of the validation methods class
                    ValidationMethods validation = new ValidationMethods();

                    // Check if the password meets the requirements
                    if (!validation.PasswordRequirements(model.Password))
                    {
                        ModelState.AddModelError("Password", "Password must contain at least 8 characters, 1 uppercase, 1 lowercase, 1 number and 1 special character.");
                    }
                    else
                    {
                        // Get the user id from the session
                        var uid = HttpContext.Session.GetString("_UserToken");

                        // For password changes, we need to use the Admin SDK since client-side auth doesn't support password updates
                        // This is a limitation of Firebase client-side authentication
                        var userArgs = new UserRecordArgs
                        {
                            Uid = uid,
                            Password = model.Password
                        };

                        await _auth.UpdateUserAsync(userArgs);

                        _logger.LogInformation("Password changed successfully for user: {UserId}", uid);
                        return RedirectToAction("Home", "Farmers");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                ModelState.AddModelError("", "An error occurred while changing the password. Please try again.");
            }

            return View(model);
        }
    }
}
