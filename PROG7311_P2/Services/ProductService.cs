using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Models;
using PROG7311_P2.Utils;

namespace PROG7311_P2.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllProductsForEmployeeAsync(string? authToken = null)
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync();
                return products.Select(p => new ProductViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    DateSupplied = p.DateSupplied,
                    TypeName = p.Type?.Type ?? "Unknown",
                    Email = p.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products for employee");
                throw;
            }
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllProductsForFarmerAsync(string farmerEmail, string? authToken = null)
        {
            try
            {
                var products = await _productRepository.GetProductsByFarmerAsync(farmerEmail);
                return products.Select(p => new ProductViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    DateSupplied = p.DateSupplied,
                    TypeName = p.Type?.Type ?? "Unknown",
                    Email = p.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for farmer: {FarmerEmail}", farmerEmail);
                throw;
            }
        }

        public async Task<IEnumerable<ProductViewModel>> FilterProductsAsync(string farmer, string startDate, string endDate, string typeName, int userRole, string? currentUserEmail = null, string? authToken = null)
        {
            try
            {
                IEnumerable<Product> products;

                // Get base products
                if (userRole == (int)Roles.Employee)
                {
                    products = await _productRepository.GetAllProductsAsync();
                }
                else
                {
                    if (string.IsNullOrEmpty(currentUserEmail))
                    {
                        throw new ArgumentException("Farmer email is required for farmer role");
                    }
                    products = await _productRepository.GetProductsByFarmerAsync(currentUserEmail);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(farmer) && farmer != "All" && userRole == (int)Roles.Employee)
                {
                    products = products.Where(p => p.Email == farmer);
                }

                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(startDate, out var start) && DateTime.TryParse(endDate, out var end))
                    {
                        products = products.Where(p => p.DateSupplied >= start && p.DateSupplied <= end);
                    }
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParse(startDate, out var start))
                    {
                        products = products.Where(p => p.DateSupplied >= start);
                    }
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(endDate, out var end))
                    {
                        products = products.Where(p => p.DateSupplied <= end);
                    }
                }

                if (!string.IsNullOrEmpty(typeName) && typeName != "All")
                {
                    products = products.Where(p => p.Type?.Type == typeName);
                }

                return products.Select(p => new ProductViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    DateSupplied = p.DateSupplied,
                    TypeName = p.Type?.Type ?? "Unknown",
                    Email = p.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                throw;
            }
        }

        public async Task<Product> CreateProductAsync(Product product, string typeName, string? authToken = null)
        {
            try
            {
                // Validate product data
                if (!await ValidateProductAsync(product, authToken))
                {
                    throw new InvalidOperationException("Product validation failed");
                }

                // Get product type ID
                var productTypes = await _productRepository.GetAllProductTypesAsync();
                var productType = productTypes.FirstOrDefault(pt => pt.Type == typeName);
                
                if (productType == null)
                {
                    throw new ArgumentException($"Product type '{typeName}' not found");
                }

                product.TypeId = productType.TypeId;
                product.DateSupplied = DateTime.Now;

                _logger.LogInformation("Creating product: {ProductName} for {Email}", product.ProductName, product.Email);

                return await _productRepository.AddProductAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductName}", product.ProductName);
                throw;
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetProductTypeSelectListAsync(string? authToken = null)
        {
            try
            {
                var productTypes = await _productRepository.GetAllProductTypesAsync();
                return productTypes.Select(pt => new SelectListItem
                {
                    Text = pt.Type,
                    Value = pt.Type
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product type select list");
                throw;
            }
        }

        public async Task<bool> ValidateProductAsync(Product product, string? authToken = null)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    _logger.LogWarning("Product validation failed: Product name is empty");
                    return false;
                }

                if (product.Quantity <= 0)
                {
                    _logger.LogWarning("Product validation failed: Quantity must be greater than 0");
                    return false;
                }

                if (product.Price <= 0)
                {
                    _logger.LogWarning("Product validation failed: Price must be greater than 0");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(product.Email))
                {
                    _logger.LogWarning("Product validation failed: Email is required");
                    return false;
                }

                // Check if product name already exists for this farmer
                try
                {
                    var existingProducts = await _productRepository.GetProductsByFarmerAsync(product.Email, authToken);
                    if (existingProducts.Any(p => p.ProductName.Equals(product.ProductName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Product validation failed: Product name already exists for farmer: {ProductName}", product.ProductName);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not check for existing products due to Firebase connection issue - skipping duplicate check");
                    // Continue with validation even if we can't check for duplicates
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product");
                return false;
            }
        }


    }
} 