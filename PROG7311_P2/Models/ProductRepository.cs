using Microsoft.Extensions.Logging;
using PROG7311_P2.Services;

namespace PROG7311_P2.Models
{
    public class ProductRepository : IProductRepository
    {
        private readonly IFirebaseDatabaseService _firebaseService;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(IFirebaseDatabaseService firebaseService, ILogger<ProductRepository> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByFarmerAsync(string farmerEmail, string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetProductsByFarmerAsync(farmerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for farmer: {FarmerEmail}", farmerEmail);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate, string? farmerEmail = null, string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetProductsByDateRangeAsync(startDate, endDate, farmerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by date range: {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(string typeName, string? farmerEmail = null, string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetProductsByTypeAsync(typeName, farmerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by type: {TypeName}", typeName);
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id, string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<Product> AddProductAsync(Product product, string? authToken = null)
        {
            try
            {
                return await _firebaseService.AddProductAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {ProductName}", product.ProductName);
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product, string? authToken = null)
        {
            try
            {
                return await _firebaseService.UpdateProductAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", product.Id);
                throw;
            }
        }

        public async Task DeleteProductAsync(int id, string? authToken = null)
        {
            try
            {
                await _firebaseService.DeleteProductAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProductType>> GetAllProductTypesAsync(string? authToken = null)
        {
            try
            {
                return await _firebaseService.GetAllProductTypesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product types");
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(int id, string? authToken = null)
        {
            try
            {
                return await _firebaseService.ProductExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists: {ProductId}", id);
                throw;
            }
        }
    }
} 