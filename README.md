# Sports Court & Slot Booking System

A C# console application for managing sports courts, hourly time slots, customer bookings, and cash payments using SQL Server and ADO.NET.

## Overview

The Sports Court & Slot Booking System provides a menu-driven interface for an administrator to manage court information, create hourly slots, process customer bookings, and track payment records.

The project follows a layered architecture that separates models, services, repositories, and the console interface.

## Features

### Court Management
- Add new courts.
- View all courts.
- Search for a court by ID.
- Update court details and active status.

### Slot Management
- Create hourly time slots for a court.
- View all slots.
- Search for a slot by ID.
- View available slots for a selected court and date.
- Track slot availability and reservation status.

### Booking Management
- Create bookings for customers.
- View all bookings.
- Search bookings by ID.
- Cancel eligible bookings.
- Calculate the total booking amount based on the court's hourly rate and booking duration.
- Associate bookings with reserved time slots.

### Payment Management
- View all payment records.
- Search for a payment using the booking ID.
- Check payment status.
- Record cash collection.
- Track payment status and collection time.

## Technology Stack

- **Language:** C#
- **Platform:** .NET
- **Database:** Microsoft SQL Server Express
- **Database Access:** ADO.NET
- **SQL Server Provider:** Microsoft.Data.SqlClient
- **Development Environment:** Visual Studio Code
- **Version Control:** Git and GitHub

## Architecture

The application uses a layered architecture.

```text
Program.cs
    |
    v
Service Layer
    |
    v
Repository Layer
    |
    v
DatabaseHelper
    |
    v
SQL Server Database
```

### Project Layers

| Layer | Responsibility |
|---|---|
| Models | Represent courts, slots, bookings, and payments. |
| Services | Apply business rules and validate application operations. |
| Repositories | Execute SQL commands and retrieve database records. |
| Helpers | Provide shared database connection functionality. |
| Program.cs | Display menus, collect input, and coordinate operations. |

## Database

**Database name:** `SportsCourtBookingDB`

The main tables are:

- `Courts` — stores court information and hourly rates.
- `Slots` — stores court dates, time ranges, and availability.
- `Bookings` — stores customer booking information and booking status.
- `Payments` — stores payment amounts, methods, statuses, and collection details.

The database uses primary keys, foreign keys, unique constraints, and check constraints to help maintain data integrity.

Booking operations coordinate the booking record, reserved slots, and associated payment record. Database transactions are used for important multi-step operations.

## Prerequisites

Before running the application, ensure you have:

- A compatible .NET SDK.
- SQL Server Express installed and running.
- SQL Server Management Studio (SSMS), recommended for database setup.
- Visual Studio Code or another compatible C# development environment.
- The `Microsoft.Data.SqlClient` package referenced by the project.

## Setup and Installation

### 1. Clone the repository

```bash
git clone <YOUR_GITHUB_REPOSITORY_URL>
```

Replace the placeholder with the actual HTTPS URL of this repository.

### 2. Open the project

Open the cloned project folder in Visual Studio Code and navigate to the directory containing the `.csproj` file.

### 3. Set up the database

Create the `SportsCourtBookingDB` database in SQL Server.

Create the required `Courts`, `Slots`, `Bookings`, and `Payments` tables, including their relationships and constraints, using your project's SQL scripts or existing database schema.

Insert sample court records if needed.

### 4. Configure the database connection

Open `DatabaseHelper.cs` and verify that the connection string matches your SQL Server instance.

The current development configuration uses:

```csharp
@"Server=.\SQLEXPRESS;Database=SportsCourtBookingDB;" +
@"Trusted_Connection=True;TrustServerCertificate=True;";
```

Update the server name if your SQL Server instance uses a different name.

### 5. Restore dependencies

Run this command in the terminal from the project directory:

```bash
dotnet restore
```

### 6. Build the project

```bash
dotnet build
```

Resolve any build errors before running the application.

### 7. Run the application

```bash
dotnet run
```

The main menu should appear in the terminal.

## Example Booking Calculation

Suppose a customer books Badminton Court 1 for two consecutive hours.

| Detail | Value |
|---|---|
| Court ID | 3 |
| Hourly rate | ₹400 |
| Booking duration | 2 hours |
| Total amount | ₹800 |

**Calculation:**

`Total Amount = Hourly Rate × Number of Hours`

`₹400 × 2 = ₹800`

The requested hourly slots must exist and be available for the selected court and date.

## Version Control

Git is used to track development changes. The project history includes commits for the initial setup, model classes, repository updates, Court Management, updates to `Program.cs`, and subsequent feature development.

Detailed information about these commits is available in [`docs.md`](docs.md).

## Future Improvements

Possible future enhancements include:

- Administrator authentication.
- A dedicated booking confirmation operation.
- Booking and payment reports.
- Automated generation of time slots.
- Additional payment methods.
- Automated unit and integration tests.
- A graphical or web-based user interface.

These are potential enhancements and are not claimed as implemented features.

## Author

**Developer:** Thanya Thavasimani

## License

No license has been specified for this repository. Add a license file if you intend to publish the project for reuse by others.

