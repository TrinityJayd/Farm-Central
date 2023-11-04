using System;
using System.Collections.Generic;

namespace PROG7311_P2.Models;

public partial class UserRole
{
    public int Id { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Farmer> Farmers { get; set; } = new List<Farmer>();
}


