using PROG7311_P2.Models;

namespace PROG7311_P2.Models
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(string? authToken = null);
        Task<IEnumerable<Product>> GetProductsByFarmerAsync(string farmerEmail, string? authToken = null);
        Task<IEnumerable<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate, string? farmerEmail = null, string? authToken = null);
        Task<IEnumerable<Product>> GetProductsByTypeAsync(string typeName, string? farmerEmail = null, string? authToken = null);
        Task<Product?> GetProductByIdAsync(int id, string? authToken = null);
        Task<Product> AddProductAsync(Product product, string? authToken = null);
        Task<Product> UpdateProductAsync(Product product, string? authToken = null);
        Task DeleteProductAsync(int id, string? authToken = null);
        Task<IEnumerable<ProductType>> GetAllProductTypesAsync(string? authToken = null);
        Task<bool> ProductExistsAsync(int id, string? authToken = null);
    }
} 