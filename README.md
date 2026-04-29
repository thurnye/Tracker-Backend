# Donations Tracker Backend

A .NET Web API backend for a financial and donations tracking application. The backend uses an N-tier structure with separate API, Core, Database, and optional Tests projects.

## Tech Stack

- .NET 8 Web API
- ASP.NET Core
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT authentication
- FluentValidation
- Redis caching
- Swagger / OpenAPI

## Project Structure

```text
DonationTracker.sln
├── DonationsTracker.Api/
│   ├── Program.cs
│   ├── Controllers/
│   ├── Middlewares/
│   ├── Properties/launchSettings.json
│   └── DonationsTracker.Api.csproj
├── DonationsTracker.Core/
│   ├── Helpers/
│   ├── Interfaces/
│   ├── Services/
│   ├── Validators/
│   └── DonationsTracker.Core.csproj
├── DonationsTracker.DB/
│   ├── Entities/
│   ├── Migrations/
│   ├── Repositories/
│   ├── Seed/
│   └── DonationsTracker.DB.csproj
└── DonationsTracker.Tests/
    └── DonationsTracker.Tests.csproj


## Prerequisites

Install the following before running the project:

- .NET 8 SDK
- SQL Server or SQL Server running through Docker
- Redis or Redis running through Docker
- Visual Studio Code
- C# extension for VS Code

Check your installed .NET versions:

```bash
dotnet --list-sdks
dotnet --list-runtimes
```

The API currently targets:

```text
net8.0
```

## Getting Started

Clone the repository:

```bash
git clone https://github.com/thurnye/Tracker-Backend.git
cd Tracker-Backend
```

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build DonationTracker.sln
```

Run the API:

```bash
dotnet run --project DonationsTracker.Api/DonationsTracker.Api.csproj
```

The API should be available at:

```text
http://localhost:5107
https://localhost:7002
```

Swagger should be available at:

```text
http://localhost:5107/swagger
https://localhost:7002/swagger
```

## Database Setup

The application uses Entity Framework Core migrations. On startup, the app can create the database and apply migrations.

Default database name:

```text
DonationTracker
```

If migrations need to be applied manually, run:

```bash
dotnet ef database update --project DonationsTracker.DB --startup-project DonationsTracker.Api
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

## Seeding Data

The project includes seed logic for default users, categories, wallets, goals, budgets, and transactions.

Important seeding order:

```text
Users
Categories
Wallets
Budgets / Goals
Transactions
```

Transactions depend on valid foreign keys, especially:

```text
Transactions.UserId -> AspNetUsers.Id
Transactions.CategoryId -> Categories.Id
Transactions.WalletId -> Wallets.Id
```

When seeding transactions, make sure the seeded user ID and seeded category IDs are passed into the transaction seeder instead of using hardcoded IDs from JSON files.

Example flow:

```csharp
var seededUser = await UserSeeder.SeedUserAsync(userManager);

var seededCategories = await CategorySeeder.SeedDefaultCategoriesAsync(
    context,
    seededUser.Id
);

await TransactionSeeder.SeedTransactionsAsync(
    context,
    seededUser.Id,
    seededCategories
);
```

This prevents foreign key errors such as:

```text
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Transactions_AspNetUsers_UserId".
```

## Running in Visual Studio Code

Open the repository root in VS Code:

```bash
code .
```

Use the existing debug configuration:

```text
Run DonationsTracker API
```

If VS Code asks for a debugger, choose:

```text
C#
```

Do not choose Node.js, Chrome, Python, or Attach for this API.

## Useful Commands

Run only the API project:

```bash
dotnet run --project DonationsTracker.Api/DonationsTracker.Api.csproj
```

Build only the API project:

```bash
dotnet build DonationsTracker.Api/DonationsTracker.Api.csproj
```

Clean generated files:

```bash
dotnet clean
```

Remove test project from the solution if it is not needed:

```bash
dotnet sln DonationTracker.sln remove DonationsTracker.Tests/DonationsTracker.Tests.csproj
```

## Git Ignore Notes

Build artifacts should not be committed. The `.gitignore` should include:

```gitignore
[Bb]in/
[Oo]bj/
out/
```

If `bin` or `obj` files were already tracked by Git, untrack them with:

```bash
find . -type d \( -name bin -o -name obj \) -prune -exec git rm -r --cached {} \;
git add .gitignore
git commit -m "Ignore .NET build artifacts"
```
