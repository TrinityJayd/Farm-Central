using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Models;
using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using System.Text;

namespace PROG7311_P2.Services
{
    public class FirebaseDatabaseService : IFirebaseDatabaseService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly ILogger<FirebaseDatabaseService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _databaseUrl;

        public FirebaseDatabaseService(IConfiguration configuration, ILogger<FirebaseDatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _databaseUrl = _configuration["Firebase:DatabaseUrl"];
            if (string.IsNullOrEmpty(_databaseUrl))
            {
                throw new InvalidOperationException("Firebase Database URL is not configured");
            }
            
            _firebaseClient = new FirebaseClient(_databaseUrl);
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                // Bypass Firebase client entirely and get raw JSON
                var url = $"{_databaseUrl}/products.json";
                var response = await _httpClient.GetStringAsync(url);
                
                // Parse the JSON array manually
                var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);
                var products = new List<Product>();
                
                for (int i = 0; i < jsonArray.Length; i++)
                {
                    var element = jsonArray[i];
                    if (element.ValueKind != JsonValueKind.Null)
                    {
                        var product = JsonSerializer.Deserialize<Product>(element.GetRawText());
                        if (product != null)
                            {
                            product.Id = i;
                            products.Add(product);
                            }
                        }
                    }

                if (products.Any())
                {
                await PopulateProductTypesAsync(products);
                _logger.LogInformation("Retrieved {Count} products from Firebase", products.Count);
                return products;
                }
                else
                {
                    _logger.LogInformation("No products found in Firebase");
                    return new List<Product>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products from Firebase");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByFarmerAsync(string farmerEmail)
        {
            try
            {
                // Firebase returns a direct array, so use OnceAsync<Product>() to get each item
                var response = await _firebaseClient
                    .Child("products")
                    .OrderBy("Email")
                    .EqualTo(farmerEmail)
                    .OnceAsync<Product>();

                var products = new List<Product>();
                
                foreach (var item in response)
                {
                    if (item.Object != null)
                    {
                        // Set the Id from the array index (item.Key)
                        if (int.TryParse(item.Key, out int id))
                        {
                            item.Object.Id = id;
                        }
                        products.Add(item.Object);
                            }
                        }

                if (products.Any())
                {
                await PopulateProductTypesAsync(products);
                _logger.LogInformation("Retrieved {Count} products for farmer {FarmerEmail}", products.Count, farmerEmail);
                return products;
                }
                else
                {
                    _logger.LogInformation("No products found for farmer {FarmerEmail}", farmerEmail);
                    return new List<Product>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for farmer {FarmerEmail}", farmerEmail);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate, string? farmerEmail = null)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                var filteredProducts = allProducts.Where(p => 
                    p.DateSupplied >= startDate && p.DateSupplied <= endDate);

                if (!string.IsNullOrEmpty(farmerEmail))
                {
                    filteredProducts = filteredProducts.Where(p => p.Email == farmerEmail);
                }

                return filteredProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by date range");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(string typeName, string? farmerEmail = null)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                var filteredProducts = allProducts.Where(p => p.Type?.Type == typeName);

                if (!string.IsNullOrEmpty(farmerEmail))
                {
                    filteredProducts = filteredProducts.Where(p => p.Email == farmerEmail);
                }

                return filteredProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by type: {TypeName}", typeName);
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var productSnapshot = await _firebaseClient
                    .Child("products")
                    .Child(id.ToString())
                    .OnceSingleAsync<Product>();

                if (productSnapshot != null)
                {
                    productSnapshot.Id = id;
                    
                    // Load product type and populate the Type property
                    if (productSnapshot.TypeId.HasValue)
                    {
                        var productTypes = await GetAllProductTypesAsync();
                        var productType = productTypes.FirstOrDefault(pt => pt.TypeId == productSnapshot.TypeId.Value);
                        if (productType != null)
                        {
                            productSnapshot.Type = productType;
                        }
                    }
                }

                return productSnapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            try
            {
                // Get the next available ID
                var allProducts = await GetAllProductsAsync();
                var maxId = allProducts.Any() ? allProducts.Max(p => p.Id) : 0;
                product.Id = maxId + 1;

                await _firebaseClient
                    .Child("products")
                    .Child(product.Id.ToString())
                    .PutAsync(JsonSerializer.Serialize(product));

                _logger.LogInformation("Product added successfully: {ProductName} with ID {ProductId}", 
                    product.ProductName, product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {ProductName}", product.ProductName);
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            try
            {
                await _firebaseClient
                    .Child("products")
                    .Child(product.Id.ToString())
                    .PutAsync(JsonSerializer.Serialize(product));

                _logger.LogInformation("Product updated successfully: {ProductId}", product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductName}", product.ProductName);
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                await _firebaseClient
                    .Child("products")
                    .Child(id.ToString())
                    .DeleteAsync();

                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProductType>> GetAllProductTypesAsync()
        {
            try
            {
                // Bypass Firebase client entirely and get raw JSON
                var url = $"{_databaseUrl}/productTypes.json";
                var response = await _httpClient.GetStringAsync(url);
                
                // Parse the JSON array manually
                var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);
                var productTypes = new List<ProductType>();
                
                for (int i = 0; i < jsonArray.Length; i++)
                {
                    var element = jsonArray[i];
                    if (element.ValueKind != JsonValueKind.Null)
                    {
                        var productType = JsonSerializer.Deserialize<ProductType>(element.GetRawText());
                        if (productType != null)
                            {
                            productType.TypeId = i;
                            productTypes.Add(productType);
                        }
                    }
                }

                if (productTypes.Any())
                {
                    _logger.LogInformation("Retrieved {Count} product types from Firebase", productTypes.Count);
                    return productTypes;
                }
                else
                {
                    _logger.LogInformation("No product types found in Firebase, will create defaults");
                }

                // Create default product types if none exist
                _logger.LogInformation("Creating default product types");
                var defaultProductTypes = await CreateDefaultProductTypesAsync();
                return defaultProductTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProductTypesAsync");
                throw;
            }
        }

        public async Task<ProductType> AddProductTypeAsync(ProductType productType)
        {
            try
            {
                await _firebaseClient
                    .Child("productTypes")
                    .Child(productType.TypeId.ToString())
                    .PutAsync(JsonSerializer.Serialize(productType));

                _logger.LogInformation("Product type added successfully: {TypeName}", productType.Type);
                return productType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product type: {TypeName}", productType.Type);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
                return product != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists: {ProductId}", id);
                throw;
            }
        }



        public async Task<IEnumerable<Farmer>> GetAllFarmersAsync()
        {
            try
            {
                // Bypass Firebase client entirely and get raw JSON
                var url = $"{_databaseUrl}/farmers.json";
                var response = await _httpClient.GetStringAsync(url);
                
                // Parse the JSON array manually
                var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);
                var farmers = new List<Farmer>();
                
                for (int i = 0; i < jsonArray.Length; i++)
                {
                    var element = jsonArray[i];
                    if (element.ValueKind != JsonValueKind.Null)
                    {
                        var farmer = JsonSerializer.Deserialize<Farmer>(element.GetRawText());
                        if (farmer != null)
                            {
                            farmer.Id = i.ToString();
                            farmers.Add(farmer);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} farmers from Firebase", farmers.Count);
                return farmers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving farmers from Firebase");
                throw;
            }
        }

        public async Task<Farmer?> GetFarmerByEmailAsync(string email)
        {
            try
            {
                var farmerSnapshot = await _firebaseClient
                    .Child("farmers")
                    .OrderBy("Email")
                    .EqualTo(email)
                    .OnceAsync<Farmer>();

                var farmer = farmerSnapshot.FirstOrDefault();
                if (farmer != null)
                {
                    farmer.Object.Id = farmer.Key;
                }

                return farmer?.Object;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving farmer by email: {Email}", email);
                throw;
            }
        }

        public async Task<Farmer> AddFarmerAsync(Farmer farmer)
        {
            try
            {
                var key = await _firebaseClient
                    .Child("farmers")
                    .PostAsync(JsonSerializer.Serialize(farmer));

                farmer.Id = key.Key;
                _logger.LogInformation("Farmer added successfully: {FarmerName} with ID {FarmerId}", 
                    farmer.Name, farmer.Id);
                return farmer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding farmer: {FarmerName}", farmer.Name);
                throw;
            }
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                // Bypass Firebase client entirely and get raw JSON
                var url = $"{_databaseUrl}/employees.json";
                var response = await _httpClient.GetStringAsync(url);
                
                // Parse the JSON array manually
                var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);
                var employees = new List<Employee>();
                
                for (int i = 0; i < jsonArray.Length; i++)
                {
                    var element = jsonArray[i];
                    if (element.ValueKind != JsonValueKind.Null)
                    {
                        var employee = JsonSerializer.Deserialize<Employee>(element.GetRawText());
                        if (employee != null)
                            {
                            employee.Id = i.ToString();
                            employees.Add(employee);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} employees from Firebase", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees from Firebase");
                throw;
            }
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                var employeeSnapshot = await _firebaseClient
                    .Child("employees")
                    .OrderBy("Email")
                    .EqualTo(email)
                    .OnceAsync<Employee>();

                var employee = employeeSnapshot.FirstOrDefault();
                if (employee != null)
                {
                    employee.Object.Id = employee.Key;
                }

                return employee?.Object;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by email: {Email}", email);
                throw;
            }
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            try
            {
                var employeeJson = JsonSerializer.Serialize(employee);
                var key = await _firebaseClient
                    .Child("employees")
                    .PostAsync(employeeJson);
                
                employee.Id = key.Key;
                _logger.LogInformation("Employee added successfully: {EmployeeName} with ID {EmployeeId}", 
                    employee.Name, employee.Id);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding employee: {EmployeeName}", employee.Name);
                throw;
            }
        }

        private async Task PopulateProductTypesAsync(List<Product> products)
        {
            try
            {
                if (!products.Any()) return;

                // Get all product types
                var productTypes = await GetAllProductTypesAsync();
                var productTypesDict = productTypes.ToDictionary(pt => pt.TypeId, pt => pt);

                // Populate the Type property for each product
                foreach (var product in products)
                {
                    if (product.TypeId.HasValue && productTypesDict.ContainsKey(product.TypeId.Value))
                    {
                        product.Type = productTypesDict[product.TypeId.Value];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating product types");
                // Don't throw here, as we don't want to break product loading if type loading fails
            }
        }

        private async Task<List<ProductType>> CreateDefaultProductTypesAsync()
        {
            try
            {
                var defaultProductTypes = new List<ProductType>
                {
                    new ProductType { TypeId = 1, Type = "Vegetables" },
                    new ProductType { TypeId = 2, Type = "Fruits" },
                    new ProductType { TypeId = 3, Type = "Grains" },
                    new ProductType { TypeId = 4, Type = "Dairy" },
                    new ProductType { TypeId = 5, Type = "Meat" },
                    new ProductType { TypeId = 6, Type = "Poultry" },
                    new ProductType { TypeId = 7, Type = "Fish" },
                    new ProductType { TypeId = 8, Type = "Herbs" },
                    new ProductType { TypeId = 9, Type = "Nuts" },
                    new ProductType { TypeId = 10, Type = "Other" }
                };

                foreach (var productType in defaultProductTypes)
                {
                    try
                    {
                        await _firebaseClient
                            .Child("productTypes")
                            .Child(productType.TypeId.ToString())
                            .PutAsync(JsonSerializer.Serialize(productType));
                        
                        _logger.LogInformation("Successfully created product type: {TypeName} with ID {TypeId}", 
                            productType.Type, productType.TypeId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create product type: {TypeName} with ID {TypeId}", 
                            productType.Type, productType.TypeId);
                        throw;
                    }
                }

                _logger.LogInformation("Created {Count} default product types", defaultProductTypes.Count);
                return defaultProductTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default product types");
                throw;
            }
        }
    }
} 