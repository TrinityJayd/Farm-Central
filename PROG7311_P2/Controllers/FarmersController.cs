using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_P2.Models;
using FirebaseAdmin.Auth;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace PROG7311_P2.Controllers
{
    public class FarmersController : Controller
    {
        private readonly Progp2Context _context;
        private readonly FirebaseAdmin.Auth.FirebaseAuth auth;
        private readonly FirebaseAuthProvider authUser;

        public FarmersController(Progp2Context context)
        {
            _context = context;
            auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            authUser = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyChY24wbV9v40wp4wNzIlFM9BX1auUZQUk"));
        }


        public IActionResult Home()
        {
            //get uid
            var uid = HttpContext.Session.GetString("_UserToken");

            //check if the user is logged in
            if (uid != null)
            {
                //get the role
                var role = HttpContext.Session.GetInt32("_UserRole");

                //check in the database if the user is a farmer
                var farmer = _context.Farmers.FirstOrDefault(e => e.Id == uid);

                //if the user is a farmer
                if (farmer != null)
                {
                    if (farmer.UserRoleId == role)
                    {
                        //set the uid
                        HttpContext.Session.SetString("_UserToken", uid);
                        return View();
                    }
                }

                //if the user is not a farmer
                return RedirectToAction("Home", "Employees");

            }

            //if the user is not logged in
            return RedirectToAction("Login", "Employees");


        }

        // GET: Farmers/Create
        public IActionResult Create()
        {
            //ONLY EMPLOYEES CAN CREATE FARMERS

            //get the uid
            var uid = HttpContext.Session.GetString("_UserToken");
            
            if (uid != null)
            {
                //check in the database if the user is an employee
                var employee = _context.Employees.FirstOrDefault(e => e.Id == uid);
                if (employee != null)
                {
                    //get the role
                    var role = HttpContext.Session.GetInt32("_UserRole");
                    if (employee.UserRoleId == role)
                    {
                        return View();
                    }
                }
                //if the user is not an employee
                return RedirectToAction("Home", "Farmers");
            }
            //if the user is not logged in
            return RedirectToAction("Login", "Employees");
        }

        // POST: Farmers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Phone,Email")] Farmer farmer)
        {
            if (ModelState.IsValid)
            {
                //create a new instance of the validation methods class
                ValidationMethods validation = new ValidationMethods();

                //check if the email is taken
                if (IsEmailTaken(farmer.Email).Result == true)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                }
                //check if the email is valid
                else if (!validation.IsEmailValid(farmer.Email))
                {
                    ModelState.AddModelError("Email", "Email must be valid.");
                }
                //check if the phone number is valid
                else if (!validation.IsPhoneNumberValid(farmer.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone number must be 10 digits long.");
                }
                //check if the name is valid
                else if (!validation.OnlyLetters(farmer.Name))
                {
                    ModelState.AddModelError("Name", "Name must only contain letters.");
                }
                //check if the email is in the organization
                else if (!validation.IsEmailInOrganization(farmer.Email))
                {
                    ModelState.AddModelError("Email", "Email must be in the organization.");
                }
                else
                {
                    //For now the password will be set to Password1* until the user logs in and changes it

                    //PasswordGenerator passwordGenerator = new PasswordGenerator();
                    string password = "Password1*";

                    //create a new instance of the firebase auth class
                    FirebaseAdmin.Auth.FirebaseAuth auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;

                    //create a new user
                    var user = new UserRecordArgs
                    {
                        Email = farmer.Email,
                        Password = password,
                        //set the email verified to false so that the last login will be null
                        //this will force the user to change their password when they log in
                        EmailVerified = false,
                        Disabled = false
                    };
                    
                    //create the user
                    var createdUser = await auth.CreateUserAsync(user);

                    //get the uid of the created user
                    var userUid = createdUser.Uid;

                    //create a new farmer
                    Farmer newFarmer = new()
                    {
                        Id = userUid,
                        Name = farmer.Name,
                        Address = farmer.Address,
                        Phone = farmer.Phone,
                        Email = farmer.Email,
                        UserRoleId = (int) Roles.Farmer,
                    };

                    //add the farmer to the database
                    _context.Farmers.Add(newFarmer);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Home", "Employees");
                }              
            }
            return View(farmer);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            //get uid
            var uid = HttpContext.Session.GetString("_UserToken");
            if (uid != null)
            {
                //get role
                var role = HttpContext.Session.GetInt32("_UserRole");

                //check if the user is a farmer
                var farmer = _context.Farmers.FirstOrDefault(e => e.Id == uid);
                if(farmer != null)
                {
                    if (farmer.UserRoleId == role)
                    {
                        return View();
                    }
                }
                return RedirectToAction("Home", "Employees");
            }
            return RedirectToAction("Login", "Employees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(LoginRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                //get the uid
                var userUID = HttpContext.Session.GetString("_UserToken");

                //get the user
                var user = await auth.GetUserAsync(userUID);

                ValidationMethods validation = new ValidationMethods();

                //check if the password meets the requirements
                if (!validation.PasswordRequirements(model.Password))
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.");
                    return View();
                }
                else
                {
                    // Update the user's password
                    var updateRequest = new UserRecordArgs
                    {
                        Uid = user.Uid,
                        Password = model.Password
                    };

                    //update the user
                    await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(updateRequest);


                    //sign the user in with their email and password
                    var fbAuthLink = await authUser.SignInWithEmailAndPasswordAsync(user.Email, model.Password);

                    //get the uid
                    var uid = fbAuthLink.User.LocalId;

                    if (uid != null)
                    {
                        //set the uid
                        HttpContext.Session.SetString("_UserToken", uid);

                        //the farmer must login with their new password
                        return RedirectToAction("Login", "Employees");
                    }
                }

            }

            return View();
        }

        
        public async Task<bool> IsEmailTaken(string email)
        {
            //check if the email is taken
            var farmer = await _context.Farmers.FirstOrDefaultAsync(e => e.Email == email);
            if (farmer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
