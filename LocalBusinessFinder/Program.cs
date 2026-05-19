using LocalBusinessFinder.Components;
using LocalBusinessFinder.Data;
using LocalBusinessFinder.Hubs;
using LocalBusinessFinder.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Optional: run with profile "Sqlite" or set ASPNETCORE_ENVIRONMENT_SQLITE=1 to use SQLite
if (builder.Configuration["ASPNETCORE_ENVIRONMENT_SQLITE"] == "1"
    || args.Contains("--sqlite", StringComparer.OrdinalIgnoreCase))
{
    builder.Configuration.AddJsonFile("appsettings.Sqlite.json", optional: false, reloadOnChange: true);
}

builder.Services.AddApplicationDatabase(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<BusinessService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<AdminService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
    {
        logger.LogError(ex,
            "Database startup failed. For SQL Server, ensure LocalDB is installed (Visual Studio installer → SQL Server Express LocalDB). " +
            "Or run with: dotnet run -- --sqlite");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<TrackingHub>("/hubs/tracking");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
