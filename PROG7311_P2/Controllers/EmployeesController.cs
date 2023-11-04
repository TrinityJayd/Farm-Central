using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PROG7311_P2.Models;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.AspNetCore.Http;

namespace PROG7311_P2.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly Progp2Context _context;
        private readonly FirebaseAuthProvider auth;

        public EmployeesController(Progp2Context context)
        {
            _context = context;

            //initialize the firebase authentication provider
            auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyChY24wbV9v40wp4wNzIlFM9BX1auUZQUk"));
        }


        public IActionResult Home()
        {
            //get the user id from the session
            var uid = HttpContext.Session.GetString("_UserToken");
            
            if (uid != null)
            {
                var role = HttpContext.Session.GetInt32("_UserRole");

                //check in the database if the user is an employee
                var employee = _context.Employees.FirstOrDefault(e => e.Id == uid);

                //if the user is an employee, return the view
                if(employee != null)
                {
                    if (employee.UserRoleId == role)
                    {
                        HttpContext.Session.SetString("_UserToken", uid);
                        return View();
                    }
                }

                //if the user is a farmer, redirect them to the farmer home page
                return RedirectToAction("Home", "Farmers");
            }

            //if the user is not logged in, redirect them to the login page
            return RedirectToAction("Login", "Employees");


        }

        public IActionResult Create()
        {
            //if the employee registration button is clicked, log the user out and return the view
            Logout();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password")] LoginRegisterModel employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //create a new instance of the validation methods class
                    ValidationMethods validation = new ValidationMethods();

                    //check if the email already exists and if they do, show error message
                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
                    {
                        ModelState.AddModelError("Email", "User with this email already exists.");
                    }
                    //check if the password meets the requirements and if it doesnt, show error message
                    else if (!validation.PasswordRequirements(employee.Password))
                    {
                        ModelState.AddModelError("Password", "Password must contain at least 8 characters, 1 uppercase, 1 lowercase, 1 number and 1 special character.");
                    }
                    //check if the email is valid and if it isnt, show error message
                    else if (!validation.IsEmailValid(employee.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be valid.");
                    }
                    //check if the name only contains letters and if it doesnt, show error message
                    else if (!validation.OnlyLetters(employee.Name))
                    {
                        ModelState.AddModelError("Name", "Name must only contain letters.");
                    }
                    //check if the email is in the organization and if it isnt, show error message
                    else if (!validation.IsEmailInOrganization(employee.Email))
                    {
                        ModelState.AddModelError("Email", "Email must be in the organization.");
                    }
                    else
                    {
                        //create a new user in firebase authentication
                        var fbAuthLink = await auth.CreateUserWithEmailAndPasswordAsync(employee.Email, employee.Password);

                        //get the user id from firebase authentication
                        var uid = fbAuthLink.User.LocalId;
                      
                        if (uid != null)
                        {
                            //create a new employee object excluding the password field
                            Employee newEmployee = new Employee
                            {
                                Id = uid,
                                Name = employee.Name,
                                Email = employee.Email,
                                UserRoleId = (int) Roles.Employee
                            };

                            //add the new employee to the database
                            _context.Employees.Add(newEmployee);
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Login", "Employees");
                        }
                    }

                }
                catch (Firebase.Auth.FirebaseAuthException ex)
                {
                    //Code Attribution
                    //Title: How to Integrate Firebase in ASP NET Core MVC
                    //https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application
                    var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                    ModelState.AddModelError(String.Empty, firebaseEx.Error.Message);
                    return View(employee);
                }
            }

            return View(employee);
        }

        //Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginRegisterModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //get the employee from the database
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);

                    //get the farmer from the database
                    var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.Email == user.Email);

                    //if the employee exists, log them in
                    if (employee != null)
                    {
                        //Code Attribution
                        //Title: How to Integrate Firebase in ASP NET Core MVC
                        //https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application

                        //log in an existing user
                        var fbAuthLink = await auth
                                        .SignInWithEmailAndPasswordAsync(user.Email, user.Password);
                        var uid = fbAuthLink.User.LocalId;

                        if (uid != null)
                        {
                            //set the user id to the logged in user
                            HttpContext.Session.SetString("_UserToken", uid);
                            
                            //set the user role to the logged in user
                            HttpContext.Session.SetInt32("_UserRole", (int) Roles.Employee);
                            return RedirectToAction("Home", "Employees");
                        }
                    }
                    //if the farmer exists, log them in
                    else if (farmer != null)
                    {
                        //Code Attribution
                        //Title: How to Integrate Firebase in ASP NET Core MVC
                        //https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application

                        //create a new instance of the google credential class
                        GoogleCredential credential = GoogleCredential.FromFile("prog7311-f7f97-firebase-adminsdk-k5k9q-ecac9ca3ef.json");

                        //initialize the firebase app
                        FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;

                        //assign credential to the default instance
                        if (firebaseApp == null)
                        {
                            firebaseApp = FirebaseApp.Create(new AppOptions
                            {
                                Credential = credential,
                            });
                        }

                        //get the firebase authentication instance
                        FirebaseAdmin.Auth.FirebaseAuth adminAuth = FirebaseAdmin.Auth.FirebaseAuth.GetAuth(firebaseApp);

                        //get the farmer user record
                        UserRecord farmerUser = adminAuth.GetUserAsync(farmer.Id).Result;

                        // Check if the user has logged in for the first time
                        var lastLogin = farmerUser.UserMetaData.LastSignInTimestamp;

                        //if they have logged in for the first time, redirect them to the change password page
                        if (lastLogin == null)
                        {
                            var fbAuthLink = await auth
                                        .SignInWithEmailAndPasswordAsync(user.Email, user.Password);
                            var uid = fbAuthLink.User.LocalId;

                            if (uid != null)
                            {
                                //set the user id to the logged in user
                                HttpContext.Session.SetString("_UserToken", uid);

                                //set the user role to the logged in user
                                HttpContext.Session.SetInt32("_UserRole", (int)Roles.Farmer);

                                //redirect user to change password page
                                return RedirectToAction("ChangePassword", "Farmers");
                            }
                        }
                        else
                        {
                            //log in an existing user
                            var fbAuthLink = await auth
                                            .SignInWithEmailAndPasswordAsync(user.Email, user.Password);
                            var uid = fbAuthLink.User.LocalId;

                            if (uid != null)
                            {
                                //set the user id to the logged in user
                                HttpContext.Session.SetString("_UserToken", uid);

                                //set the user role to the logged in user
                                HttpContext.Session.SetInt32("_UserRole", (int)Roles.Farmer);
                                return RedirectToAction("Home", "Farmers");

                            }
                        }
                    }
                    //if the employee and farmer do not exist, show error message
                    else if (employee == null && farmer == null)
                    {
                        ModelState.AddModelError("Email", "User with this email does not exist.");
                    }
                }
                catch (Firebase.Auth.FirebaseAuthException ex)
                {
                    //Code Attribution
                    //Title: How to Integrate Firebase in ASP NET Core MVC
                    //https://www.freecodespot.com/blog/firebase-in-asp-net-core-mvc/#II_Create_and_Setup_a_new_ASPNET_Core_MVC_Application
                    var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                    ModelState.AddModelError(String.Empty, firebaseEx.Error.Message);
                    return View(user);
                }
            }
            
            return View();
        }

        //Logout
        public IActionResult Logout()
        {
            //clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }


    }
}
