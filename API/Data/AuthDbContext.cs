using API.Models;
using Microsoft.EntityFrameworkCore;
using API.Common.Enums;

namespace API.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // User related
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserEmail> UserEmails { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;

    // Account related
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AccountSession> AccountSessions { get; set; } = null!;

    // Organization related
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationRole> OrganizationRoles { get; set; } = null!;

    // Developer and Application related
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationAccount> ApplicationAccounts { get; set; } = null!;
    public DbSet<ApplicationSession> ApplicationSessions { get; set; } = null!;

    // Token
    public DbSet<Token> Tokens { get; set; } = null!;

    // Audit and Monitoring
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            // Indexes for performance
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasFilter("\"PhoneNumber\" IS NOT NULL");

            // Cascade delete for user-owned entities
            entity.HasMany(u => u.Emails)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom column types and names
            entity.Property(u => u.Username).HasColumnType("varchar(50)").HasColumnName("login_name");
            entity.Property(u => u.State).HasConversion<string>().HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.LastLoginAt).HasColumnType("timestamp");
        });

        // Account configurations
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");

            // Indexes
            entity.HasIndex(a => new { a.UserId, a.IsPersonal })
                .IsUnique()
                .HasFilter("is_personal = true")
                .HasDatabaseName("IX_OnePersonalAccountPerUser");
            entity.HasIndex(a => a.AccountName);

            // Many-to-many relationships
            entity.HasMany(a => a.AuthorizedDevelopers)
                .WithMany(d => d.AuthorizedAccounts)
                .UsingEntity(j => j.ToTable("account_developer_authorizations"));

            entity.HasMany(a => a.Roles)
                .WithMany(r => r.Members)
                .UsingEntity(j => j.ToTable("account_organization_roles"));

            // Custom columns
            entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.IsPersonal).HasColumnName("is_personal");
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Organization configurations
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");

            // Indexes
            entity.HasIndex(o => o.Name);

            // Cascade delete behavior
            entity.HasMany(o => o.Roles)
                .WithOne(r => r.Organization)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom columns
            entity.Property(o => o.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Developer configurations
        modelBuilder.Entity<Developer>(entity =>
        {
            entity.ToTable("developers");

            // Indexes
            entity.HasIndex(d => d.Name);
            entity.HasIndex(d => new { d.Type, d.OrganizationId });

            // Organization relationship
            entity.HasOne(d => d.Organization)
                .WithMany(o => o.Developers)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Custom columns
            entity.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Application configurations
        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("applications");

            // Indexes
            entity.HasIndex(a => a.Name);
            entity.HasIndex(a => new { a.DeveloperId, a.Status });

            // Developer relationship with cascade delete
            entity.HasOne(a => a.Developer)
                .WithMany(d => d.Applications)
                .HasForeignKey(a => a.DeveloperId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom columns
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ApiKey configurations
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys");

            entity.HasIndex(k => k.ApplicationId);
            entity.HasIndex(k => k.Status);
            entity.HasIndex(k => k.ExpiresAt);

            // Application relationship
            entity.HasOne(k => k.Application)
                .WithMany(a => a.ApiKeys)
                .HasForeignKey(k => k.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(k => k.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(k => k.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(k => k.LastUsedAt).HasColumnType("timestamp");
            entity.Property(k => k.ExpiresAt).HasColumnType("timestamp");
        });

        // Token configurations
        modelBuilder.Entity<Token>(entity =>
        {
            entity.ToTable("tokens");

            // Indexes
            entity.HasIndex(t => t.TokenString).IsUnique();
            entity.HasIndex(t => t.ExpiresAt);
            entity.HasIndex(t => new { t.Target, t.UserId });

            // Relationships based on TokenTarget
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional relationships
            entity.HasOne(t => t.ApplicationAccount)
                .WithMany()
                .HasForeignKey(t => t.ApplicationAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Custom columns
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Target).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(t => t.ExpiresAt).HasColumnType("timestamp");
        });

        // Session configurations
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");

            entity.HasIndex(s => s.UserId);
            entity.Property(s => s.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(s => s.LastUsedAt).HasColumnType("timestamp");
        });

        modelBuilder.Entity<AccountSession>(entity =>
        {
            entity.ToTable("account_sessions");

            entity.HasIndex(s => new { s.UserId, s.AccountId });
            entity.Property(s => s.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(s => s.LastUsedAt).HasColumnType("timestamp");
        });

        modelBuilder.Entity<ApplicationSession>(entity =>
        {
            entity.ToTable("application_sessions");

            entity.HasIndex(s => new { s.UserId, s.AccountId, s.ApplicationId });
            entity.Property(s => s.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // AuditLog configurations
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasIndex(a => new { a.EntityName, a.EntityId });
            entity.HasIndex(a => a.UserId);
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.HasIndex(n => new { n.AccountId, n.ApplicationId });

            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(n => n.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(n => n.ReadAt).HasColumnType("timestamp");
        });

        // ApplicationAccount configurations
        modelBuilder.Entity<ApplicationAccount>(entity =>
        {
            entity.ToTable("application_accounts");

            entity.HasIndex(aa => aa.AccountId);
            entity.HasIndex(aa => aa.ApplicationId);
        });

        // OrganizationRole configurations
        modelBuilder.Entity<OrganizationRole>(entity =>
        {
            entity.ToTable("organization_roles");

            entity.HasIndex(r => r.OrganizationId);
        });

        // UserEmail configurations
        modelBuilder.Entity<UserEmail>(entity =>
        {
            entity.ToTable("user_emails");

            entity.HasIndex(ue => ue.UserId);
        });
    }
}