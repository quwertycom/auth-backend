using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database.Entities.Account;
using API.Infrastructure.Database.Entities.Application;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.Developer;
using API.Infrastructure.Database.Entities.Organization;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;

namespace API.Infrastructure.Database;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // Entities.Account
    public DbSet<Account> Accounts { get; set; } = null!;

    // Entities.Application
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationAccount> ApplicationAccounts { get; set; } = null!;

    // Entities.Authentication
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Token> Tokens { get; set; } = null!;

    // Entities.Developer
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<DeveloperAccount> DeveloperAccounts { get; set; } = null!;

    // Entities.Organization
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationRole> OrganizationRoles { get; set; } = null!;

    // Entities.User
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<EmailAddress> UserEmails { get; set; } = null!;
    public DbSet<PhoneNumber> UserPhoneNumbers { get; set; } = null!;

    // Entities.Verification
    public DbSet<VerifyEmailSession> VerifyEmailSessions { get; set; } = null!;
    public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; } = null!;


    
}
