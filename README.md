## Proiect Laborator VVSS - Platforma de inchirieri auto

### Membri echipa:
- Dumitrasc Ciprian - 10LF331
- Bangala Costin - 10LF331
- Ilie Paul - 10LF331
- Barbu Calin - 10LF331

## Project Structure
The project is organized using Layered Architecture and contains the following components:

### 1. CarRental.Domain
Contains the core entities of the application:
 - Car Entity
 - Id (int) – Primary key
 - LicensePlate (string) – License plate number (unique, required)
 - Model (string) – Car model (minimum 4 characters)
 - ManufacturingYear (int) – Year of manufacture
 - DailyRate (decimal) – Daily rental rate
 - IsAvailable (bool) – Availability status

### 2. CarRental.Repository
Implements data access using Entity Framework Core:
 - CarRentalDbContext – EF Core context
 - ICarRepository – Repository interface for Car
 - CarRepository – Repository implementation for Car

### 3. CarRental.Service
Contains business logic, validations, and exception handling:
 - ICarService – Service interface for Car
 - CarService – Implementation with validations:
    - LicensePlate is required (not null, empty, or whitespace)
    - DailyRate > 0
    - ManufacturingYear <= CurrentYear
    - Model length >= 4 characters

### 4. CarRental.Tests
Project dedicated to unit testing and mocking:
 - CarServiceTests – 32 unit tests for CarService (using Mo)
 #### Car Management Implementation (by Ciprian)
Implemented Features
 - CRUD operations for Car
 - Business logic validations:
   - License plate is required and must be unique
   - Daily rate must be > 0
   - Manufacturing year must be ≤ current year
   - Model length >= 4 characters
 - Logging for all operations
 - Exception handling

### Technologies Used
 - .NET 8.0
 - Entity Framework Core 8.0
 - Moq (for mocking)
 - xUnit (for unit testing)
 - Microsoft.Extensions.Logging
