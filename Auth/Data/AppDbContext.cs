using Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<ApplicationAccount> ApplicationAccounts { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Developer> Developers { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AccountSession> AccountSessions { get; set; }
    public DbSet<ApplicationSession> ApplicationSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("auth");

        // Organization
        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Accounts)
            .WithOne(a => a.Organization)
            .HasForeignKey(a => a.OrganizationId);

        // User
        modelBuilder.Entity<User>()
            .HasMany(u => u.Accounts)
            .WithOne(u => u.User)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Devices)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId);

        // Account
        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Organization)
            .WithMany(o => o.Accounts)
            .HasForeignKey(a => a.OrganizationId);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Sessions)
            .WithOne(s => s.Account)
            .HasForeignKey(s => s.AccountId);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Applications)
            .WithOne(a => a.Account)
            .HasForeignKey(a => a.AccountId);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Developers)
            .WithMany(d => d.Accounts)
            .UsingEntity(j => j.ToTable("DeveloperAccounts"));

        // ApplicationAccount
        modelBuilder.Entity<ApplicationAccount>()
            .HasKey(aa => new { aa.ApplicationId, aa.AccountId });

        modelBuilder.Entity<ApplicationAccount>()
            .HasOne(aa => aa.Application)
            .WithMany(a => a.Accounts)
            .HasForeignKey(aa => aa.ApplicationId);

        modelBuilder.Entity<ApplicationAccount>()
            .HasOne(aa => aa.Account)
            .WithMany(a => a.Applications)
            .HasForeignKey(aa => aa.AccountId);

        modelBuilder.Entity<ApplicationAccount>()
            .HasMany(aa => aa.Sessions)
            .WithOne(s => s.Account)
            .HasForeignKey(s => new { s.ApplicationId, s.AccountId });

        // Application
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Developer)
            .WithMany(d => d.Applications)
            .HasForeignKey(a => a.DeveloperId);

        modelBuilder.Entity<Application>()
            .HasMany(a => a.Accounts)
            .WithOne(a => a.Application)
            .HasForeignKey(a => a.ApplicationId);

        modelBuilder.Entity<Application>()
            .HasMany(a => a.Sessions)
            .WithOne(s => s.Application)
            .HasForeignKey(s => s.ApplicationId);

        // Device
        modelBuilder.Entity<Device>()
            .HasOne(d => d.User)
            .WithMany(u => u.Devices)
            .HasForeignKey(d => d.UserId);

        modelBuilder.Entity<Device>()
            .HasMany(d => d.AccountSessions)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId);

        modelBuilder.Entity<Device>()
            .HasMany(d => d.ApplicationSessions)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId);

        modelBuilder.Entity<Device>()
            .HasMany(d => d.UserSessions)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId);

        // Developer
        modelBuilder.Entity<Developer>()
            .HasMany(d => d.Applications)
            .WithOne(a => a.Developer)
            .HasForeignKey(a => a.DeveloperId);

        modelBuilder.Entity<Developer>()
            .HasMany(d => d.Accounts)
            .WithMany(a => a.Developers)
            .UsingEntity(j => j.ToTable("DeveloperAccounts"));

        // UserSession
        modelBuilder.Entity<UserSession>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<UserSession>()
            .HasOne(s => s.Device)
            .WithMany(d => d.UserSessions)
            .HasForeignKey(s => s.DeviceId);

        // AccountSession
        modelBuilder.Entity<AccountSession>()
            .HasOne(s => s.Account)
            .WithMany(a => a.Sessions)
            .HasForeignKey(s => s.AccountId);

        modelBuilder.Entity<AccountSession>()
            .HasOne(s => s.Device)
            .WithMany(d => d.AccountSessions)
            .HasForeignKey(s => s.DeviceId);

        // ApplicationSession
        modelBuilder.Entity<ApplicationSession>()
            .HasOne(s => s.Account)
            .WithMany(aa => aa.Sessions)
            .HasForeignKey(s => new { s.ApplicationId, s.AccountId });

        modelBuilder.Entity<ApplicationSession>()
            .HasOne(s => s.Application)
            .WithMany(a => a.Sessions)
            .HasForeignKey(s => s.ApplicationId);

        modelBuilder.Entity<ApplicationSession>()
            .HasOne(s => s.Device)
            .WithMany(d => d.ApplicationSessions)
            .HasForeignKey(s => s.DeviceId);
    }
}
