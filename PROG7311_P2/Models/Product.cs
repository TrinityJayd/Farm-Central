using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_P2.Models;

public partial class Product
{
    public int? Id { get; set; }

    public string ProductName { get; set; }

    //display quantity as an integer using data annotations
    [DisplayFormat(DataFormatString = "{0:0.##}")]
    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    
    [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
    public DateTime DateSupplied { get; set; }

    public int? TypeId { get; set; }

    public string Email { get; set; }
    public virtual Farmer? EmailNavigation { get; set; }
    public virtual ProductType? Type { get; set; }

}
