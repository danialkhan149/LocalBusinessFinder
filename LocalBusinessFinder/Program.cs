using LocalBusinessFinder.Components;
using LocalBusinessFinder.Data;
using LocalBusinessFinder.Endpoints;
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

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = googleAuthNSection["ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
    });

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
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

var app = builder.Build();

// Run DB initialization in the background so it doesn't block the app from starting up quickly
_ = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database startup failed.");
    }
});

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
app.MapAccountEndpoints();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<TrackingHub>("/hubs/tracking");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
