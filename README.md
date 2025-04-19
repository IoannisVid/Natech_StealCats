# StealCats

This is an ASP.NET Core Web API application which stores Cat Images from thecatapi.com and can also retrieve them.
The project follows a clean architecture using repository patterns with SQL Server database.

## Features

- `POST /api/cats/fetch` – Retrieves 25 cat images from thecatapi.com and stores them alongside their breed temperament
- `GET /api/cats/{id}` – Retrieves a cat by each ID
- `GET /api/cats` – Retrieves cats with paging support, can also retrieve cats with a specific tag

---

## Prerequisites

- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

---

## Getting Started

### 1. Clone the Repository

```bash
    git clone https://github.com/IoannisVid/Natech_StealCats
```
### 2. Configure the Database

In appsettings.json enter your connectionString
```json
"ConnectionStrings": {
  "connectionString": "Server=YOUR_SERVER_NAME;Database=StealCats;Trusted_Connection=True;TrustServerCertificate=True;"
```
Apply Migrations to create the database schema
```CLI
dotnet ef database update
```
### 3. Build the Application
```CLI
dotnet build
```
### 4. Run the Application
```CLI
dotnet run
```
The api will be available at
http://localhost:5228
Swagger UI will be available at https://localhost:5228/swagger
