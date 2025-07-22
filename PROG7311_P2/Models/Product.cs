using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_P2.Models;

public partial class Product
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string ProductName { get; set; } = string.Empty;

    //display quantity as an integer using data annotations
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [DisplayFormat(DataFormatString = "{0:0.##}")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Date supplied is required")]
    [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
    public DateTime DateSupplied { get; set; }

    public int? TypeId { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    public virtual Farmer? EmailNavigation { get; set; }
    public virtual ProductType? Type { get; set; }
}
