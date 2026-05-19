using LocalBusinessFinder.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ServiceCategory> Categories => Set<ServiceCategory>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<LocationPing> LocationPings => Set<LocationPing>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Business>()
            .HasOne(b => b.Owner)
            .WithOne(u => u.OwnedBusiness)
            .HasForeignKey<Business>(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceRequest>()
            .HasOne(r => r.Business)
            .WithMany(b => b.Requests)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceRequest>()
            .HasOne(r => r.Review)
            .WithOne(rv => rv.ServiceRequest)
            .HasForeignKey<Review>(rv => rv.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Business)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.ServiceRequest)
            .WithMany(sr => sr.Messages)
            .HasForeignKey(m => m.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LocationPing>()
            .HasOne(lp => lp.ServiceRequest)
            .WithMany(sr => sr.LocationPings)
            .HasForeignKey(lp => lp.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Business>().Property(b => b.HourlyRate).HasPrecision(18, 2);
        builder.Entity<ServiceRequest>().Property(r => r.AgreedPrice).HasPrecision(18, 2);
        builder.Entity<ChatMessage>().Property(m => m.OfferAmount).HasPrecision(18, 2);
    }
}
