using System;
using System.Collections.Generic;

namespace PROG7311_P2.Models;

public partial class Employee
{
    public string Id { get; set; } 

    public string Name { get; set; } 

    public string Email { get; set; } 
    public int UserRoleId { get; set; }

    public virtual UserRole UserRole { get; set; } 
}
