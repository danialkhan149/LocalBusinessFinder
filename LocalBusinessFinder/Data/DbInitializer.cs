using LocalBusinessFinder.Constants;
using LocalBusinessFinder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dbSettings = scope.ServiceProvider.GetRequiredService<DatabaseSettings>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.ApplyMigrationsAsync(dbSettings.Provider);

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await SeedAdminAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedDemoDataAsync(context, userManager);
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@localfinder.com";
        var admin = await userManager.FindByEmailAsync(email);
        
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = "System Admin",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            admin.IsActive = true;
            await userManager.UpdateAsync(admin);
            
            // Forcibly reset password in case it was changed/corrupted
            var token = await userManager.GeneratePasswordResetTokenAsync(admin);
            await userManager.ResetPasswordAsync(admin, token, "Admin@123");
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        context.Categories.AddRange(
            new ServiceCategory { Name = "Car Mechanic", Description = "Auto repair and maintenance", Icon = "bi-wrench" },
            new ServiceCategory { Name = "Plumber", Description = "Plumbing services", Icon = "bi-droplet" },
            new ServiceCategory { Name = "Electrician", Description = "Electrical work", Icon = "bi-lightning" },
            new ServiceCategory { Name = "Cleaner", Description = "Home and office cleaning", Icon = "bi-brush" },
            new ServiceCategory { Name = "Tutor", Description = "Private tutoring", Icon = "bi-book" },
            new ServiceCategory { Name = "Salon", Description = "Hair and beauty", Icon = "bi-scissors" },
            new ServiceCategory { Name = "Catering", Description = "Food and events", Icon = "bi-cup-hot" },
            new ServiceCategory { Name = "Moving", Description = "Moving and delivery", Icon = "bi-truck" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // 1. Ensure the demo user exists and is located at Rawalpindi/Islamabad
        var userDemo = await userManager.FindByEmailAsync("user@demo.com");
        if (userDemo == null)
        {
            await CreateUserAsync(userManager, "user@demo.com", "Demo User", AppRoles.User, 33.6260, 73.0714);
        }
        else
        {
            userDemo.LastLatitude = 33.6260;
            userDemo.LastLongitude = 73.0714;
            await userManager.UpdateAsync(userDemo);
        }

        // 2. Define dynamic specifications for all 8 categories
        var demoSpecs = new[]
        {
            new { Email = "owner1@demo.com", Name = "Mike's Auto Repair", Category = "Car Mechanic", Rate = 2500m, Lat = 33.5984, Lng = 73.0441, Address = "Saddar, Rawalpindi", Desc = "Expert car mechanics — engine, brakes, AC, diagnostics.", Phone = "+92-300-1111111" },
            new { Email = "owner2@demo.com", Name = "Quick Fix Plumbing", Category = "Plumber", Rate = 1800m, Lat = 33.6007, Lng = 73.0679, Address = "Chandni Chowk, Rawalpindi", Desc = "24/7 emergency plumbing, leaks, installations.", Phone = "+92-300-2222222" },
            new { Email = "owner_electrician@demo.com", Name = "Pindi Sparky", Category = "Electrician", Rate = 1500m, Lat = 33.6310, Lng = 73.0720, Address = "6th Road, Rawalpindi", Desc = "Professional home electrical wiring, switchboard repairs, and appliance installations.", Phone = "+92-300-3333333" },
            new { Email = "owner_cleaner@demo.com", Name = "Shiny Spaces Cleaners", Category = "Cleaner", Rate = 1000m, Lat = 33.6391, Lng = 73.0772, Address = "Commercial Market, Rawalpindi", Desc = "Top-tier home deep cleaning, carpet washing, sofa cleaning, and office sanitization.", Phone = "+92-300-4444444" },
            new { Email = "owner_tutor@demo.com", Name = "Apex Academy Tutors", Category = "Tutor", Rate = 2000m, Lat = 33.6295, Lng = 73.0664, Address = "Satellite Town, Rawalpindi", Desc = "Expert home tuition for Mathematics, Physics, Chemistry, and English (O/A levels & Matric).", Phone = "+92-300-5555555" },
            new { Email = "owner_salon@demo.com", Name = "Grace Beauty Salon", Category = "Salon", Rate = 1200m, Lat = 33.6420, Lng = 73.0805, Address = "Satellite Town, Rawalpindi", Desc = "Premium bridal makeup, hair styling, facials, manicures, and salon services at home.", Phone = "+92-300-6666666" },
            new { Email = "owner_catering@demo.com", Name = "Rawal Catering & Events", Category = "Catering", Rate = 4000m, Lat = 33.5204, Lng = 73.0910, Address = "Bahria Town, Rawalpindi", Desc = "Traditional Pakistani and continental dishes catering for weddings, corporate, and private parties.", Phone = "+92-300-7777777" },
            new { Email = "owner_moving@demo.com", Name = "Pindi Movers & Packers", Category = "Moving", Rate = 3000m, Lat = 33.5651, Lng = 73.0182, Address = "Adyala Road, Rawalpindi", Desc = "Safe house/office relocation, furniture dismantling, packing, and loading transport services.", Phone = "+92-300-8888888" }
        };

        // 3. Process each demo specification
        foreach (var spec in demoSpecs)
        {
            var cat = await context.Categories.FirstOrDefaultAsync(c => c.Name == spec.Category);
            if (cat == null) continue;

            // Find or create owner user
            var ownerUser = await userManager.FindByEmailAsync(spec.Email);
            if (ownerUser == null)
            {
                ownerUser = await CreateUserAsync(userManager, spec.Email, spec.Name, AppRoles.BusinessOwner, spec.Lat, spec.Lng);
            }
            else
            {
                ownerUser.LastLatitude = spec.Lat;
                ownerUser.LastLongitude = spec.Lng;
                await userManager.UpdateAsync(ownerUser);
            }

            if (ownerUser == null) continue;

            // Find or create business
            var biz = await context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerUser.Id || b.Name == spec.Name);
            if (biz == null)
            {
                biz = new Business
                {
                    OwnerId = ownerUser.Id,
                    CategoryId = cat.Id,
                    Name = spec.Name,
                    Description = spec.Desc,
                    Address = spec.Address,
                    Phone = spec.Phone,
                    Latitude = spec.Lat,
                    Longitude = spec.Lng,
                    HourlyRate = spec.Rate,
                    IsApproved = true,
                    IsOnline = true,
                    Rating = 4.7,
                    ReviewCount = 15
                };
                context.Businesses.Add(biz);
            }
            else
            {
                // Forcibly align location, category, rate and details
                biz.Latitude = spec.Lat;
                biz.Longitude = spec.Lng;
                biz.Address = spec.Address;
                biz.HourlyRate = spec.Rate;
                biz.Description = spec.Desc;
                biz.CategoryId = cat.Id;
                biz.IsApproved = true;
                biz.IsOnline = true;
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string role,
        double lat,
        double lng)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            LastLatitude = lat,
            LastLongitude = lng
        };
        var result = await userManager.CreateAsync(user, "Demo@123");
        if (!result.Succeeded) return null;
        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
