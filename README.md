# FarmCentral

FarmCentral is a web application that allows local farmers to work together and sell their products under one roof. It provides a platform for tracking incoming and outgoing stock and associating each item with the respective farmer. The application includes user authentication and different roles for farmers and employees. 

Live site: [Farm Central](https://st10083735.azurewebsites.net/)

## Test Accounts

Farmer: johndoe@farmcentral.com

Password: Password12*

Employee: trinity@farmcentral.com

Password: Password1*

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Usage](#usage)
- [Database](#database)

## Features

- User Roles: The website provides two different user roles: farmer and employee. Each role has specific permissions and access to user-specific information.
- User Authentication: Farmers and employees are required to log in to access the website's features and their respective information.
- Farmer Database: The website maintains a database of farmers and their associated products. Farmers can manage their product inventory and information.
- Employee Functionality: Logged-in employees have the ability to add new farmers to the database, manage the overall system, and perform administrative tasks.
- Product Management: Logged-in farmers can add new products to their profiles in the database, update product information, and keep track of their inventory.
- Product Filtering: Logged-in employees can view a list of all products supplied by a specific farmer and filter the displayed list based on the date range or product type.

## Prerequisites

To run the FarmCentral application locally, you need the following dependencies:

- Visual Studio 2022
- .NET 6.0

Additionally, the following NuGet packages were used in the application:

- AspNetCore.Firebase.Authentication
- EntityFramework
- Firebase.Auth
- FirebaseAdmin
- Microsoft.AspNetCore.Session
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.VisualStudio.Web.CodeGeneration.Design

## Setup Instructions

To set up the FarmCentral application on your local machine, follow these steps:

1. Clone the project repository from GitHub.
2. Open the project in Visual Studio 2022.
3. Add your connection string in appsettings.json
4. Add the your firebase admin json file to the PROG7311_P2 direcotry.
5. Build the project to restore dependencies and compile the code.
6. Run the application locally using the debugging tools provided by Visual Studio.

## Usage

- User Roles:
  - Farmer: Farmers can log in to the website, add new products to their profile, and view their own product listings.
  - Employee: Employees can log in to the website, add new farmers to the database, and view the list of products supplied by specific farmers.

- Logging In:
  - Visit the FarmCentral website in your browser.
  - Click on the "Login" button.
  - Enter your credentials based on the assigned user role (farmer or employee).
  - Upon successful login, you will be redirected to the respective user dashboard.

- Adding a Farmer (Employee Only):
  - Log in to the website as an employee.
  - Navigate to the "Farmers" section.
  - Click on the "Add New Farmer" button.
  - Fill in the required information for the new farmer and submit the form.
  - The new farmer will be added to the database.

- Adding a Product (Farmer Only):
  - Log in to the website as a farmer.
  - Go to your profile or product management section.
  - Click on the "Add New Product" button.
  - Provide the necessary details for the new product and submit the form.
  - The new product will be associated with your profile in the database.

- Viewing Products Supplied by a Specific Farmer (Employee Only):
  - Log in to the website as an employee.
  - Navigate to the "Farmers" section.
  - Select a specific farmer from the list.
  - The website will display the list of products ever supplied by that farmer.

- Filtering Products by Date Range or Product Type (Employee Only):
  - Log in to the website as an employee.
  - Navigate to the "Farmers" section.
  - Select a specific farmer from the list.


  - Use the available options to filter the displayed list of products by date range or product type.

## Database

The FarmCentral application uses an Azure SQL Database as the backend database for storing farmer and product information. 

[🔝 Back to Top](#farmcentral)
