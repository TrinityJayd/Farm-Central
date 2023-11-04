using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_P2.Models;

namespace PROG7311_P2.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Progp2Context _context;

        public ProductsController(Progp2Context context)
        {
            _context = context;
        }



        // GET: Products
        public IActionResult Index()
        {
            ///get the uid
            string uid = HttpContext.Session.GetString("_UserToken");

            if (uid != null)
            {
                //get the role
                int role = (int)HttpContext.Session.GetInt32("_UserRole");

                //check if the user is an employee
                if (role == (int)Roles.Employee)
                {
                    //get all the products from the database
                    return View(PopulateEmployeeFilter());
                }
                else
                {
                    //get all the farmers products from the database
                    return View(PopulateFarmerFilter(uid));
                }

            }
            else
            {
                //if the user is not logged in, redirect to the login page
                return RedirectToAction("Login", "Employees");
            }


        }

        // GET: Products/Create
        public IActionResult Create()
        {
            //ONLY FARMERS CAN CREATE PRODUCTS

            //get the uid
            string uid = HttpContext.Session.GetString("_UserToken");

            if (uid != null)
            {
                //get the role
                var role = HttpContext.Session.GetInt32("_UserRole");

                //check if the user is a farmer
                if (role == (int)Roles.Farmer)
                {
                    // Get the logged-in user's email
                    var user = HttpContext.Session.GetString("_UserToken");

                    var userEmail = _context.Farmers.Where(f => f.Id == user).FirstOrDefault().Email;

                    // Create a new instance of the Product model
                    //set the email field so that the email is already filled in
                    //preventing manipulation of the email field
                    Product product = new Product
                    {
                        Email = userEmail
                    };

                    //Add all the different names of the product types from the ProductType table to the viewbag as an IEnumerable<SelectListItem>
                    ViewBag.ProductTypes = _context.ProductTypes.Select(p => new SelectListItem
                    {
                        Text = p.Type,
                        Value = p.Type
                    });

                    return View(product);
                }
                return RedirectToAction("Home", "Employees");
            }
            return RedirectToAction("Login", "Employees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Quantity,Price,DateSupplied,Email")] Product product, string typeName)
        {
            if (ModelState.IsValid)
            {
                //get the typeid using the type name
                product.TypeId = _context.ProductTypes.Where(p => p.Type == typeName).FirstOrDefault().TypeId;

                // get the total number of products in the database
                var totalProducts = _context.Products.Count();

                // Create a new instance of the Product model
                Product newProduct = new Product
                {
                    Id = totalProducts + 1,
                    ProductName = product.ProductName,
                    Quantity = product.Quantity,
                    Price = product.Price,
                    DateSupplied = product.DateSupplied,
                    Email = product.Email,
                    TypeId = product.TypeId
                };

                // Add the new product to the database
                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction("Home", "Farmers");
            }

            return View(product);

        }

        //create the filter method to match the filter button
        public IActionResult Filter(string farmer, string startDate, string endDate, string typeName)
        {
            //get the role
            var role = HttpContext.Session.GetInt32("_UserRole");

            //check if all the fields have been left as default
            if ((farmer == "All" || farmer == null) && startDate == null && endDate == null && typeName == "All")
            {
                return RedirectToAction("Index");
            }

            //get all the products from the database
            IEnumerable<ProductViewModel> products = from p in _context.Products
                                                     join pt in _context.ProductTypes on p.TypeId equals pt.TypeId
                                                     select new ProductViewModel
                                                     {
                                                         ProductName = p.ProductName,
                                                         Quantity = p.Quantity,
                                                         Price = p.Price,
                                                         DateSupplied = p.DateSupplied,
                                                         TypeName = pt.Type,
                                                         Email = p.Email
                                                     };

            //if the user is an employee
            if (role == (int)Roles.Employee)
            {

                //they will be able to filter by farmer and see what specific products they have supplied
                if (farmer != "All")
                {
                    //from the products enumerable, get the products with the specific farmer email
                    products = from p in products
                               where p.Email == farmer
                               select p;
                }


                //they will be able to filter by date and see what products were supplied on a specific date or between a range of dates
                if (startDate != null && endDate != null)
                {
                    DateTime start = Convert.ToDateTime(startDate);
                    DateTime end = Convert.ToDateTime(endDate);

                    //get the products that were supplied between the start and end date
                    products = from p in products
                               where p.DateSupplied >= start && p.DateSupplied <= end
                               select p;

                }
                else if (startDate != null)
                {
                    DateTime start = Convert.ToDateTime(startDate);

                    //get the products that were supplied after the start date
                    products = from p in products
                               where p.DateSupplied >= start
                               select p;
                }
                else if (endDate != null)
                {
                    DateTime end = Convert.ToDateTime(endDate);

                    //get the products that were supplied before the end date
                    products = from p in products
                               where p.DateSupplied <= end
                               select p;
                }

                if (typeName != "All")
                {
                    //get the products that are of the specific type
                    products = from p in products
                               where p.TypeName == typeName
                               select p;

                }

                //if the products enumerable is not null
                if (products != null)
                {
                    //populate the employee filter
                    PopulateEmployeeFilter();
                    return View("Index", products.ToList());
                }
            }
            //if the user is a farmer
            else if (role == (int)Roles.Farmer)
            {
                //get the uid
                var user = HttpContext.Session.GetString("_UserToken");

                //get the email of the logged in farmer
                var userEmail = _context.Farmers.Where(f => f.Id == user).FirstOrDefault().Email;

                if (startDate != null && endDate != null)
                {
                    DateTime start = Convert.ToDateTime(startDate);
                    DateTime end = Convert.ToDateTime(endDate);

                    //get the farmers products that were supplied between the start and end date
                    products = from p in products
                               where p.DateSupplied >= start && p.DateSupplied <= end && p.Email == userEmail
                               select p;

                }
                else if (startDate != null)
                {
                    DateTime start = Convert.ToDateTime(startDate);

                    //get the farmers products that were supplied after the start date
                    products = from p in products
                               where p.DateSupplied >= start && p.Email == userEmail
                               select p;
                }
                else if (endDate != null)
                {
                    DateTime end = Convert.ToDateTime(endDate);

                    //get the farmers products that were supplied before the end date
                    products = from p in products
                                 where p.DateSupplied <= end && p.Email == userEmail
                                 select p;
                }

                
                if (typeName != "All")
                {
                    //get the farmers products that are of the specific type
                    products = from p in products
                               where p.TypeName == typeName && p.Email == userEmail
                               select p;
                }

                //if the products enumerable is not null
                if (products != null)
                {
                    //populate the farmer filter
                    PopulateFarmerFilter(user);
                    return View("Index", products.ToList());
                }
            }
          
            return RedirectToAction("Index");

        }

        public IEnumerable<ProductViewModel> PopulateEmployeeFilter()
        {
            IEnumerable<ProductViewModel> products;

            //return all products
            products = from p in _context.Products
                       join pt in _context.ProductTypes on p.TypeId equals pt.TypeId
                       select new ProductViewModel
                       {
                           ProductName = p.ProductName,
                           Quantity = p.Quantity,
                           Price = p.Price,
                           DateSupplied = p.DateSupplied,
                           TypeName = pt.Type,
                           Email = p.Email
                       };

            //select distinct type names of products that have been supplied
            var typeNames = (from pt in _context.ProductTypes
                             join p in _context.Products on pt.TypeId equals p.TypeId
                             select pt.Type).Distinct();
            ViewBag.TypeNames = typeNames.ToList();

            //get all the farmer emails
            var farmerEmails = _context.Farmers.Select(f => f.Email).ToList();
            ViewBag.Farmers = farmerEmails;

            return products.ToList();
        }

        public IEnumerable<ProductViewModel> PopulateFarmerFilter(string uid)
        {
            IEnumerable<ProductViewModel> products;
            //get the users email
            var userEmail = (from f in _context.Farmers
                             where f.Id == uid
                             select f.Email).FirstOrDefault();

            //return all products that the farmer has supplied
            products = from p in _context.Products
                       join pt in _context.ProductTypes on p.TypeId equals pt.TypeId
                       where p.Email == userEmail
                       select new ProductViewModel
                       {
                           ProductName = p.ProductName,
                           Quantity = p.Quantity,
                           Price = p.Price,
                           DateSupplied = p.DateSupplied,
                           TypeName = pt.Type,
                           Email = p.Email
                       };

            if (products == null)
            {
                ViewBag.TypeNames = null;
                return null;
            }
            else
            {
                //select distinct type names of products that they have supplied
                var typeNames = (from pt in _context.ProductTypes
                                 join p in _context.Products on pt.TypeId equals p.TypeId
                                 where p.Email == userEmail
                                 select pt.Type).Distinct();

                ViewBag.TypeNames = typeNames.ToList();
                return products.ToList();
            }

            

            
        }


    }

}

