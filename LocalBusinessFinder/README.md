# Local Business Finder

Blazor Web App for finding local businesses, negotiating deals, chatting, and live GPS tracking.

## Visual Studio 2022

1. Install **Visual Studio 2022** (17.12 or later) with:
   - ASP.NET and web development workload
   - **.NET 9 SDK**
   - **SQL Server Express LocalDB** (included with VS)
2. Open `LocalBusinessFinder.sln` in the parent folder (`VP Project`).
3. Set **LocalBusinessFinder** as the startup project.
4. Press **F5** (profile: `https`).

## Database (SQL Server — default)

The app uses **SQL Server LocalDB** by default:

```
Server=(localdb)\mssqllocaldb;Database=LocalBusinessFinder_Dev;...
```

On first run, EF Core applies migrations and seeds demo data automatically.

### Use full SQL Server

Edit `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "SqlServer": "Server=YOUR_SERVER;Database=LocalBusinessFinder;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Use SQLite (no SQL Server required)

```bash
dotnet run -- --sqlite
```

Or in Visual Studio, select the **Sqlite** launch profile.

### EF migrations (Package Manager Console)

```powershell
Add-Migration MigrationName -OutputDir Data\Migrations
Update-Database
```

## Run from command line

```bash
cd LocalBusinessFinder
dotnet run
```

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@localfinder.com | Admin@123 |
| Customer | user@demo.com | Demo@123 |
| Business owner | owner1@demo.com | Demo@123 |

## Configuration keys

| Key | Description |
|-----|-------------|
| `Database:Provider` | `SqlServer` (default) or `Sqlite` |
| `ConnectionStrings:SqlServer` | SQL Server connection string |
| `ConnectionStrings:Sqlite` | SQLite file path |
