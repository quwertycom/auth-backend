using Microsoft.EntityFrameworkCore;
using API.Infrastructure.Database.Entities.Account;
using API.Infrastructure.Database.Entities.Application;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.Developer;
using API.Infrastructure.Database.Entities.Organization;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Application;
using API.Shared.Enums.Account;
using API.Shared.Enums.User;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----------------------------
        // --- Account ----------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(a => a.OrganizationId);

        modelBuilder.Entity<Account>()
          .HasMany(a => a.ApplicationAccounts)
          .WithOne(aa => aa.Account)
          .HasForeignKey(aa => aa.AccountId);

        modelBuilder.Entity<Account>()
          .HasMany(a => a.DeveloperAccounts)
          .WithOne(da => da.Account)
          .HasForeignKey(da => da.AccountId);

        modelBuilder.Entity<Account>()
          .HasMany(a => a.Sessions)
          .WithOne(s => s.Account)
          .HasForeignKey(s => s.AccountId);

        modelBuilder.Entity<Account>()
          .HasMany(a => a.Roles)
          .WithMany(r => r.Members);

        // --- Properties ---

        modelBuilder.Entity<Account>()
          .HasKey(a => a.Id);

        modelBuilder.Entity<Account>()
          .Property(a => a.Name)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<Account>()
          .Property(a => a.UserId)
          .IsRequired();
        
        modelBuilder.Entity<Account>()
          .Property(a => a.OrganizationId)
          .IsRequired(false);

        modelBuilder.Entity<Account>()
          .Property(a => a.Type)
          .IsRequired()
          .HasDefaultValue(AccountType.Personal);

        modelBuilder.Entity<Account>()
          .Property(a => a.CreatedAt)
          .IsRequired();

        modelBuilder.Entity<Account>()
          .Property(a => a.RowVersion)
          .IsRowVersion();

        // ----------------------------
        // --- Application ------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Application>()
          .HasOne(a => a.Developer)
          .WithMany(d => d.Applications)
          .HasForeignKey(a => a.DeveloperId);

        modelBuilder.Entity<Application>()
          .HasMany(a => a.Accounts)
          .WithOne(aa => aa.Application)
          .HasForeignKey(aa => aa.ApplicationId);

        modelBuilder.Entity<Application>()
          .HasMany(a => a.Sessions)
          .WithOne(s => s.Application)
          .HasForeignKey(s => s.ApplicationId);

        // --- Indexes ---

        modelBuilder.Entity<Application>()
          .HasIndex(a => a.Name);

        modelBuilder.Entity<Application>()
          .HasIndex(a => a.RedirectUri);

        // --- Properties ---

        modelBuilder.Entity<Application>()
          .HasKey(a => a.Id); 

        modelBuilder.Entity<Application>()
          .Property(a => a.Name)
          .IsRequired()
          .HasMaxLength(256);

        modelBuilder.Entity<Application>()
          .Property(a => a.Description)
          .IsRequired()
          .HasMaxLength(4096)
          .HasDefaultValue("New qAuth Application...");

        modelBuilder.Entity<Application>()
          .Property(a => a.IconUrl)
          .IsRequired(false)
          .HasMaxLength(512);

        modelBuilder.Entity<Application>()
          .Property(a => a.RedirectUri)
          .IsRequired()
          .HasMaxLength(512);

        modelBuilder.Entity<Application>()
          .Property(a => a.DeveloperId)
          .IsRequired();

        modelBuilder.Entity<Application>()
          .Property(a => a.Status)
          .IsRequired()
          .HasDefaultValue(ApplicationStatus.Development);

        modelBuilder.Entity<Application>()
          .Property(a => a.CreatedAt)
          .IsRequired();
        
        // Add IsDeleted property to entities that need it
        // Then add global query filter
        modelBuilder.Entity<Application>()
          .HasQueryFilter(a => a.Status != ApplicationStatus.Removed);
        
        modelBuilder.Entity<Application>()
          .Property(a => a.RowVersion)
          .IsRowVersion();
        
        // ----------------------------
        // --- ApplicationAccount -----
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<ApplicationAccount>()
          .HasOne(aa => aa.Application)
          .WithMany(a => a.Accounts)
          .HasForeignKey(aa => aa.ApplicationId);

        modelBuilder.Entity<ApplicationAccount>()
          .HasOne(aa => aa.Account)
          .WithMany(a => a.ApplicationAccounts)
          .HasForeignKey(aa => aa.AccountId);

        modelBuilder.Entity<ApplicationAccount>()
          .HasMany(aa => aa.Sessions)
          .WithOne(s => s.ApplicationAccount)
          .HasForeignKey(s => s.ApplicationAccountId);
        
        // --- Indexes ---
        
        modelBuilder.Entity<ApplicationAccount>()
          .HasIndex(aa => new { aa.ApplicationId, aa.AccountId })
          .IsUnique();

        // --- properties ---

        modelBuilder.Entity<ApplicationAccount>()
          .HasKey(aa => aa.Id);

        modelBuilder.Entity<ApplicationAccount>()
          .Property(aa => aa.ApplicationId)
          .IsRequired();

        modelBuilder.Entity<ApplicationAccount>()
          .Property(aa => aa.AccountId)
          .IsRequired();

        modelBuilder.Entity<ApplicationAccount>()
          .Property(aa => aa.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- Session ----------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Session>()
          .HasOne(s => s.User)
          .WithMany(u => u.Sessions)
          .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<Session>()
          .HasOne(s => s.Account)
          .WithMany(a => a.Sessions)
          .HasForeignKey(s => s.AccountId);

        modelBuilder.Entity<Session>()
          .HasOne(s => s.Application)
          .WithMany(a => a.Sessions)
          .HasForeignKey(s => s.ApplicationId);

        modelBuilder.Entity<Session>()
          .HasOne(s => s.ApplicationAccount)
          .WithMany(aa => aa.Sessions)
          .HasForeignKey(s => s.ApplicationAccountId);

        modelBuilder.Entity<Session>()
          .HasMany(s => s.Tokens)
          .WithOne(t => t.Session)
          .HasForeignKey(t => t.SessionId);

        // --- Indexes ---

        modelBuilder.Entity<Session>()
          .HasIndex(s => s.CreatedAt);

        // --- Properties ---

        modelBuilder.Entity<Session>()
          .HasKey(s => s.Id);

        modelBuilder.Entity<Session>()
          .Property(s => s.IsRevoked)
          .IsRequired()
          .HasDefaultValue(false);

        modelBuilder.Entity<Session>()
          .Property(s => s.UserId)
          .IsRequired();
        
        modelBuilder.Entity<Session>()
          .Property(s => s.AccountId)
          .IsRequired(false);

        modelBuilder.Entity<Session>()
          .Property(s => s.ApplicationId)
          .IsRequired(false);
        
        modelBuilder.Entity<Session>()
          .Property(s => s.ApplicationAccountId)
          .IsRequired(false);

        modelBuilder.Entity<Session>()
          .Property(s => s.Target)
          .IsRequired();
        
        modelBuilder.Entity<Session>()
          .Property(s => s.LastUsedAt)
          .IsRequired(false)
          .HasDefaultValue(null);

        modelBuilder.Entity<Session>()
          .Property(s => s.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- Token ------------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Token>()
          .HasOne(t => t.Session)
          .WithMany(s => s.Tokens)
          .HasForeignKey(t => t.SessionId);

        modelBuilder.Entity<Token>()
          .HasOne(t => t.User)
          .WithMany()
          .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<Token>()
          .HasOne(t => t.Account)
          .WithMany()
          .HasForeignKey(t => t.AccountId);

        modelBuilder.Entity<Token>()
          .HasOne(t => t.Application)
          .WithMany()
          .HasForeignKey(t => t.ApplicationId);

        modelBuilder.Entity<Token>()
          .HasOne(t => t.ApplicationAccount)
          .WithMany()
          .HasForeignKey(t => t.ApplicationAccountId);

        modelBuilder.Entity<Token>()
          .HasOne(t => t.ParentToken)
          .WithMany()
          .HasForeignKey(t => t.ParentTokenId);

        // --- Indexes ---

        modelBuilder.Entity<Token>()
          .HasIndex(t => t.Value)
          .IsUnique();

        modelBuilder.Entity<Token>()
          .HasIndex(t => t.ExpiresAt);
        
        // --- Properties ---

        modelBuilder.Entity<Token>()
          .HasKey(t => t.Id);

        modelBuilder.Entity<Token>()
          .Property(t => t.Value)
          .IsRequired()
          .HasMaxLength(512);

        modelBuilder.Entity<Token>()
          .Property(t => t.IsRefreshed)
          .IsRequired()
          .HasDefaultValue(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.IsRevoked)
          .IsRequired()
          .HasDefaultValue(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.SessionId)
          .IsRequired();

        modelBuilder.Entity<Token>()
          .Property(t => t.UserId)
          .IsRequired();

        modelBuilder.Entity<Token>()
          .Property(t => t.AccountId)
          .IsRequired(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.ApplicationId)
          .IsRequired(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.ApplicationAccountId)
          .IsRequired(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.ParentTokenId)
          .IsRequired(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.Type)
          .IsRequired();

        modelBuilder.Entity<Token>()
          .Property(t => t.Target)
          .IsRequired();

        modelBuilder.Entity<Token>()
          .Property(t => t.ExpiresAt)
          .IsRequired(false);

        modelBuilder.Entity<Token>()
          .Property(t => t.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- Developer --------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Developer>()
          .HasMany(d => d.Accounts)
          .WithOne(da => da.Developer)
          .HasForeignKey(da => da.DeveloperId);

        modelBuilder.Entity<Developer>()
          .HasMany(d => d.Applications)
          .WithOne(a => a.Developer)
          .HasForeignKey(a => a.DeveloperId);

        modelBuilder.Entity<Developer>()
          .HasOne(d => d.Organization)
          .WithMany(o => o.Developers)
          .HasForeignKey(d => d.OrganizationId);

        // --- Properties ---

        modelBuilder.Entity<Developer>()
          .HasKey(d => d.Id);

        modelBuilder.Entity<Developer>()
          .Property(d => d.Name)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<Developer>()
          .Property(d => d.ContactEmail)
          .IsRequired()
          .HasMaxLength(256);

        modelBuilder.Entity<Developer>()
          .Property(d => d.WebsiteUrl)
          .IsRequired(false)
          .HasMaxLength(512);

        modelBuilder.Entity<Developer>()
          .Property(d => d.Description)
          .IsRequired(false)
          .HasMaxLength(4096);

        modelBuilder.Entity<Developer>()
          .Property(d => d.OrganizationId)
          .IsRequired();

        modelBuilder.Entity<Developer>()
          .Property(d => d.Type)
          .IsRequired();
          
        modelBuilder.Entity<Developer>()
          .Property(d => d.Status)
          .IsRequired();

        modelBuilder.Entity<Developer>()
          .Property(d => d.VerifiedAt)
          .IsRequired(false);

        modelBuilder.Entity<Developer>()
          .Property(d => d.CreatedAt)
          .IsRequired();

        modelBuilder.Entity<Developer>()
          .Property(d => d.RowVersion)
          .IsRowVersion();

        // ----------------------------
        // --- DeveloperAccount -------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<DeveloperAccount>()
          .HasOne(da => da.Developer)
          .WithMany(d => d.Accounts)
          .HasForeignKey(da => da.DeveloperId);

        modelBuilder.Entity<DeveloperAccount>()
          .HasOne(da => da.Account)
          .WithMany(a => a.DeveloperAccounts)
          .HasForeignKey(da => da.AccountId);

        // --- Properties ---

        modelBuilder.Entity<DeveloperAccount>()
          .HasKey(da => da.Id);

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.Name)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.DeveloperId)
          .IsRequired();

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.AccountId)
          .IsRequired();

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.Status)
          .IsRequired();

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.Type)
          .IsRequired();

        modelBuilder.Entity<DeveloperAccount>()
          .Property(da => da.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- Organization -----------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<Organization>()
          .HasMany(o => o.Members)
          .WithOne(a => a.Organization)
          .HasForeignKey(a => a.OrganizationId);

        modelBuilder.Entity<Organization>()
          .HasMany(o => o.Roles)
          .WithOne(r => r.Organization)
          .HasForeignKey(r => r.OrganizationId);

        modelBuilder.Entity<Organization>()
          .HasMany(o => o.Developers)
          .WithOne(d => d.Organization)
          .HasForeignKey(d => d.OrganizationId);

        // --- Indexes ---

        modelBuilder.Entity<Organization>()
          .HasIndex(o => o.Name);

        // --- Properties ---

        modelBuilder.Entity<Organization>()
          .HasKey(o => o.Id);

        modelBuilder.Entity<Organization>()
          .Property(o => o.Name)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<Organization>()
          .Property(o => o.Description)
          .IsRequired()
          .HasMaxLength(4096);

        modelBuilder.Entity<Organization>()
          .Property(o => o.CreatedAt)
          .IsRequired();

        modelBuilder.Entity<Organization>()
          .Property(o => o.RowVersion)
          .IsRowVersion();

        // ----------------------------
        // --- User -------------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<User>()
          .HasMany(u => u.Accounts)
          .WithOne(a => a.User)
          .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<User>()
          .HasMany(u => u.Sessions)
          .WithOne(s => s.User)
          .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<User>()
          .HasMany(u => u.PhoneNumbers)
          .WithOne(p => p.User)
          .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<User>()
          .HasMany(u => u.EmailAddresses)
          .WithOne(e => e.User)
          .HasForeignKey(e => e.UserId);

        // --- Indexes ---

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // --- Properties ---

        modelBuilder.Entity<User>()
          .HasKey(u => u.Id);
        
        modelBuilder.Entity<User>()
          .Property(u => u.Username)
          .IsRequired()
          .HasMaxLength(32);

        modelBuilder.Entity<User>()
          .Property(u => u.FirstName)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<User>()
          .Property(u => u.LastName)
          .IsRequired()
          .HasMaxLength(128);

        modelBuilder.Entity<User>()
          .Property(u => u.PasswordHash)
          .IsRequired();

        modelBuilder.Entity<User>()
          .Property(u => u.PasswordSalt)
          .IsRequired();

        modelBuilder.Entity<User>()
          .Property(u => u.BirthDate)
          .IsRequired();

        modelBuilder.Entity<User>()
          .Property(u => u.Gender)
          .IsRequired();

        modelBuilder.Entity<User>()
          .Property(u => u.State)
          .IsRequired()
          .HasDefaultValue(UserState.PendingVerification);

        modelBuilder.Entity<User>()
          .Property(u => u.CreatedAt)
          .IsRequired();

        modelBuilder.Entity<User>()
          .Property(u => u.RowVersion)
          .IsRowVersion();

        // ----------------------------
        // --- EmailAddress -----------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<EmailAddress>()
          .HasOne(e => e.User)
          .WithMany(u => u.EmailAddresses)
          .HasForeignKey(e => e.UserId);

        // --- Indexes ---

        modelBuilder.Entity<EmailAddress>()
            .HasIndex(e => e.Value)
            .IsUnique();

        // --- Properties ---

        modelBuilder.Entity<EmailAddress>()
          .HasKey(e => e.Id);

        modelBuilder.Entity<EmailAddress>()
          .Property(e => e.Value)
          .IsRequired()
          .HasMaxLength(256);

        modelBuilder.Entity<EmailAddress>()
          .Property(e => e.UserId)
          .IsRequired();

        modelBuilder.Entity<EmailAddress>()
          .Property(e => e.Type)
          .IsRequired();

        modelBuilder.Entity<EmailAddress>()
          .Property(e => e.State)
          .IsRequired()
          .HasDefaultValue(EmailState.Created);

        modelBuilder.Entity<EmailAddress>()
          .Property(e => e.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- PhoneNumber ------------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<PhoneNumber>()
          .HasOne(p => p.User)
          .WithMany(u => u.PhoneNumbers)
          .HasForeignKey(p => p.UserId);

        // --- Indexes ---

        modelBuilder.Entity<PhoneNumber>()
            .HasIndex(p => p.Value);

        // --- Properties ---

        modelBuilder.Entity<PhoneNumber>()
          .HasKey(p => p.Id);

        modelBuilder.Entity<PhoneNumber>()
          .Property(p => p.Value)
          .IsRequired()
          .HasMaxLength(16);

        modelBuilder.Entity<PhoneNumber>()
          .Property(p => p.UserId)
          .IsRequired();

        modelBuilder.Entity<PhoneNumber>()
          .Property(p => p.Type)
          .IsRequired();

        modelBuilder.Entity<PhoneNumber>()
          .Property(p => p.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- VerifyEmailSession ------
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<VerifyEmailSession>()
          .HasOne(v => v.User)
          .WithMany()
          .HasForeignKey(v => v.UserId);

        modelBuilder.Entity<VerifyEmailSession>()
          .HasOne(v => v.Email)
          .WithMany()
          .HasForeignKey(v => v.EmailId);

        // --- Indexes ---

        modelBuilder.Entity<VerifyEmailSession>()
            .HasIndex(v => v.Code);

        modelBuilder.Entity<VerifyEmailSession>()
            .HasIndex(v => v.ExpiresAt);

        // --- Properties ---

        modelBuilder.Entity<VerifyEmailSession>()
          .HasKey(v => v.Id);

        modelBuilder.Entity<VerifyEmailSession>()
          .Property(v => v.Code)
          .IsRequired();

        modelBuilder.Entity<VerifyEmailSession>()
          .Property(v => v.IsUsed)
          .IsRequired()
          .HasDefaultValue(false);

        modelBuilder.Entity<VerifyEmailSession>()
          .Property(v => v.UserId)
          .IsRequired();

        modelBuilder.Entity<VerifyEmailSession>()
          .Property(v => v.EmailId)
          .IsRequired();

        modelBuilder.Entity<VerifyEmailSession>()
          .Property(v => v.CreatedAt)
          .IsRequired();

        // ----------------------------
        // --- ResetPasswordRequest ---
        // ----------------------------

        // --- Relations ---

        modelBuilder.Entity<ResetPasswordRequest>()
          .HasOne(r => r.User)
          .WithMany()
          .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<ResetPasswordRequest>()
          .HasOne(r => r.EmailAddress)
          .WithMany()
          .HasForeignKey(r => r.EmailId);

        // --- Indexes ---

        modelBuilder.Entity<ResetPasswordRequest>()
            .HasIndex(r => r.CodeHash)
            .IsUnique();

        modelBuilder.Entity<ResetPasswordRequest>()
            .HasIndex(r => r.ExpiresAt);

        // --- Properties ---

        modelBuilder.Entity<ResetPasswordRequest>()
          .HasKey(r => r.Id);

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.CodeHash)
          .IsRequired();

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.IsUsed)
          .IsRequired()
          .HasDefaultValue(false);

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.UserId)
          .IsRequired();

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.EmailId)
          .IsRequired();

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.UsedAt)
          .IsRequired(false);

        modelBuilder.Entity<ResetPasswordRequest>()
          .Property(r => r.CreatedAt)
          .IsRequired();
    }
}
