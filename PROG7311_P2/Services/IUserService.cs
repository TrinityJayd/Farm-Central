using PROG7311_P2.Models;

namespace PROG7311_P2.Services
{
    public interface IUserService
    {
        Task<Employee?> GetEmployeeByIdAsync(string id);
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<bool> EmployeeExistsAsync(string email);
        Task<Farmer?> GetFarmerByIdAsync(string id);
        Task<Farmer?> GetFarmerByEmailAsync(string email);
        Task<Farmer> AddFarmerAsync(Farmer farmer);
        Task<bool> FarmerExistsAsync(string email);
        Task<bool> UpdateFarmerAsync(Farmer farmer);
    }
} 