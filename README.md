# Farm Central - Agricultural Management System

Farm Central is a comprehensive web application that enables local farmers to collaborate and sell their products through a unified platform. The system provides robust inventory management, user authentication, and role-based access control for farmers and employees.

## 🌟 Live Demo
**Live site:** [Farm Central](https://st10083735.azurewebsites.net/)

## 🚀 Project Overview

Farm Central is built with modern web technologies and follows industry best practices for security, scalability, and maintainability. The application uses Firebase for authentication and real-time database operations, providing a seamless user experience with automatic data synchronization.

### Key Features
- **Secure Authentication** - Firebase-based authentication with role-based access
- **Real-time Database** - Firebase Realtime Database with automatic data sync
- **Role Management** - Separate interfaces for farmers and employees
- **Product Management** - Complete CRUD operations for agricultural products
- **Advanced Filtering** - Filter products by farmer, date range, and product type
- **Responsive Design** - Mobile-friendly interface using Bootstrap 5
- **Input Validation** - Comprehensive validation and business rule enforcement
- **Error Handling** - Graceful error management with custom exceptions
- **Logging** - Structured logging with Serilog for monitoring and debugging

### User Roles

#### 👨‍🌾 Farmer Features
- Product inventory management
- Add new products with detailed information
- View personal product listings
- Filter products by date and type

#### 👨‍💼 Employee Features
- Administrative dashboard
- Farmer registration and management
- View all products across all farmers
- Advanced filtering and reporting

## 🏗️ Architecture

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Service Layer Pattern** - Business logic separation
- **Dependency Injection** - Loose coupling and testability
- **MVC Pattern** - Clean separation of concerns

### Technology Stack

#### Backend
- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core MVC** - Web application framework
- **Firebase Realtime Database** - NoSQL cloud database
- **Firebase Authentication** - User authentication service
- **Serilog** - Structured logging framework

#### Frontend
- **Bootstrap 5** - Responsive CSS framework
- **jQuery** - JavaScript library
- **Razor Views** - Server-side templating

## 🚀 Setup Instructions

### Prerequisites
- **Visual Studio 2022** (or VS Code with .NET 8.0 SDK)
- **.NET 8.0 SDK**
- **Firebase Project** (for authentication and database)

### Setup Steps

1. **Clone the Repository**
```bash
git clone https://github.com/yourusername/Farm-Central.git
cd Farm-Central
```

2. **Firebase Setup**
   - Create a Firebase project at [Firebase Console](https://console.firebase.google.com/)
   - Enable Authentication and Realtime Database
   - Download your Firebase Admin SDK JSON file
   - Place the JSON file in the project root directory
   - Update `appsettings.json` with your Firebase configuration

3. **Build and Run**
```bash
dotnet restore
dotnet build
dotnet run
```

## 📝 License

This project is licensed under the MIT License.

## 👨‍💻 Author

**Trinity** - [GitHub Profile](https://github.com/yourusername)

---

**Note:** This project was originally developed as a university assignment and has been enhanced for portfolio presentation with modern development practices, security improvements, and comprehensive documentation.
