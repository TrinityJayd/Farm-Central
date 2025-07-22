using PROG7311_P2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PROG7311_P2.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductViewModel>> GetAllProductsForEmployeeAsync(string? authToken = null);
        Task<IEnumerable<ProductViewModel>> GetAllProductsForFarmerAsync(string farmerEmail, string? authToken = null);
        Task<IEnumerable<ProductViewModel>> FilterProductsAsync(string farmer, string startDate, string endDate, string typeName, int userRole, string? currentUserEmail = null, string? authToken = null);
        Task<Product> CreateProductAsync(Product product, string typeName, string? authToken = null);
        Task<IEnumerable<SelectListItem>> GetProductTypeSelectListAsync(string? authToken = null);
        Task<bool> ValidateProductAsync(Product product, string? authToken = null);
    }
} 