using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PROG7311_P2.Models;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace PROG7311_P2.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly FirebaseAuth _auth;
        private readonly IFirebaseDatabaseService _databaseService;

        public UserService(ILogger<UserService> logger, IConfiguration configuration, IFirebaseDatabaseService databaseService)
        {
            _logger = logger;
            _configuration = configuration;
            _auth = FirebaseAuth.DefaultInstance;
            _databaseService = databaseService;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string id)
        {
            try
            {
                // First check if the user exists in Firebase Authentication
                var userRecord = await _auth.GetUserAsync(id);
                if (userRecord == null)
                {
                    _logger.LogWarning("User not found in Firebase Authentication: {UserId}", id);
                    return null;
                }

                // Then check if the user exists in the employees collection in the database
                var employee = await _databaseService.GetEmployeeByEmailAsync(userRecord.Email);
                if (employee != null)
                {
                    _logger.LogInformation("Employee found in database: {Email} with role {Role}", employee.Email, employee.UserRoleId);
                    return employee;
                }

                _logger.LogWarning("User {Email} not found in employees collection", userRecord.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by ID: {EmployeeId}", id);
                return null;
            }
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                // Check if the user exists in the employees collection in the database
                var employee = await _databaseService.GetEmployeeByEmailAsync(email);
                if (employee != null)
                {
                    _logger.LogInformation("Employee found in database: {Email} with role {Role}", employee.Email, employee.UserRoleId);
                    return employee;
                }

                _logger.LogWarning("Employee not found in database: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by email: {Email}", email);
                return null;
            }
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            try
            {
                // Update Firebase Auth display name
                var updateArgs = new UserRecordArgs
                {
                    Uid = employee.Id,
                    DisplayName = employee.Name
                };
                
                await _auth.UpdateUserAsync(updateArgs);
                
                // Add employee to the database
                await _databaseService.AddEmployeeAsync(employee);
                
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

        public async Task<bool> EmployeeExistsAsync(string email)
        {
            try
            {
                var employee = await GetEmployeeByEmailAsync(email);
                return employee != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if employee exists: {Email}", email);
                return false;
            }
        }

        public async Task<Farmer?> GetFarmerByIdAsync(string id)
        {
            try
            {
                // First check if the user exists in Firebase Authentication
                var userRecord = await _auth.GetUserAsync(id);
                if (userRecord == null)
                {
                    _logger.LogWarning("User not found in Firebase Authentication: {UserId}", id);
                    return null;
                }

                // Then check if the user exists in the farmers collection in the database
                var farmer = await _databaseService.GetFarmerByEmailAsync(userRecord.Email);
                if (farmer != null)
                {
                    _logger.LogInformation("Farmer found in database: {Email} with role {Role}", farmer.Email, farmer.UserRoleId);
                    return farmer;
                }

                _logger.LogWarning("User {Email} not found in farmers collection", userRecord.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving farmer by ID: {FarmerId}", id);
                return null;
            }
        }

        public async Task<Farmer?> GetFarmerByEmailAsync(string email)
        {
            try
            {
                // Check if the user exists in the farmers collection in the database
                var farmer = await _databaseService.GetFarmerByEmailAsync(email);
                if (farmer != null)
                {
                    _logger.LogInformation("Farmer found in database: {Email} with role {Role}", farmer.Email, farmer.UserRoleId);
                    return farmer;
                }

                _logger.LogWarning("Farmer not found in database: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving farmer by email: {Email}", email);
                return null;
            }
        }

        public async Task<Farmer> AddFarmerAsync(Farmer farmer)
        {
            try
            {
                // Farmer is already created in Firebase Auth by the controller
                // We just need to update the display name and add to database
                var updateArgs = new UserRecordArgs
                {
                    Uid = farmer.Id,
                    DisplayName = farmer.Name
                };
                
                await _auth.UpdateUserAsync(updateArgs);
                
                // Add farmer to the database using admin token
                await _databaseService.AddFarmerAsync(farmer);
                
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

        public async Task<bool> FarmerExistsAsync(string email)
        {
            try
            {
                var farmer = await GetFarmerByEmailAsync(email);
                return farmer != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if farmer exists: {Email}", email);
                return false;
            }
        }

        public async Task<bool> UpdateFarmerAsync(Farmer farmer)
        {
            try
            {
                var updateArgs = new UserRecordArgs
                {
                    Uid = farmer.Id,
                    DisplayName = farmer.Name
                };
                
                await _auth.UpdateUserAsync(updateArgs);
                
                _logger.LogInformation("Farmer updated successfully: {FarmerId}", farmer.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating farmer: {FarmerId}", farmer.Id);
                return false;
            }
        }
    }
} 