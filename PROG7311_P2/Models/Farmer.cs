using System;
using System.Collections.Generic;

namespace PROG7311_P2.Models;

public partial class Farmer
{
    public string? Id { get; set; }

    public string Name { get; set; } 

    public string Address { get; set; }

    public string Phone { get; set; } 

    public string Email { get; set; }

    public int? UserRoleId { get; set; }

    public virtual ICollection<Product>? Products { get; set; } = new List<Product>();

    public virtual UserRole? UserRole { get; set; } 
}
