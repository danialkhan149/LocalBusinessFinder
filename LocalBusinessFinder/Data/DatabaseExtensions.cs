using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Data;

public static class DatabaseExtensions
{
    public const string ProviderSqlServer = "SqlServer";
    public const string ProviderSqlite = "Sqlite";

    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? ProviderSqlServer;
        var connectionString = ResolveConnectionString(configuration, provider);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals(ProviderSqlite, StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connectionString);
            else
                options.UseSqlServer(connectionString);
        });

        services.AddSingleton(new DatabaseSettings(provider, connectionString));
        return services;
    }

    public static string ResolveConnectionString(IConfiguration configuration, string provider)
    {
        if (provider.Equals(ProviderSqlite, StringComparison.OrdinalIgnoreCase))
        {
            return configuration.GetConnectionString("Sqlite")
                ?? "Data Source=localbusinessfinder.db";
        }

        return configuration.GetConnectionString("SqlServer")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "SQL Server connection string is missing. Set ConnectionStrings:SqlServer in appsettings.");
    }

    public static async Task ApplyMigrationsAsync(this ApplicationDbContext context, string provider)
    {
        if (provider.Equals(ProviderSqlite, StringComparison.OrdinalIgnoreCase))
        {
            await context.Database.EnsureCreatedAsync();
            return;
        }

        await context.Database.MigrateAsync();
    }
}

public record DatabaseSettings(string Provider, string ConnectionString);
