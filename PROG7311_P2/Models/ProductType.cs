using System;
using System.Collections.Generic;

namespace PROG7311_P2.Models;

public partial class ProductType
{
    public int TypeId { get; set; }

    public string Type { get; set; } 

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
