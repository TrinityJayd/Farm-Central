using PROG7311_P2.Models;

namespace PROG7311_P2.Services
{
    public interface IFirebaseDatabaseService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByFarmerAsync(string farmerEmail);
        Task<IEnumerable<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate, string? farmerEmail = null);
        Task<IEnumerable<Product>> GetProductsByTypeAsync(string typeName, string? farmerEmail = null);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<ProductType>> GetAllProductTypesAsync();
        Task<ProductType> AddProductTypeAsync(ProductType productType);
        Task<bool> ProductExistsAsync(int id);
        Task<IEnumerable<Farmer>> GetAllFarmersAsync();
        Task<Farmer?> GetFarmerByEmailAsync(string email);
        Task<Farmer> AddFarmerAsync(Farmer farmer);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<Employee> AddEmployeeAsync(Employee employee);
    }
} 