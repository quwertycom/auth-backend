using API.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // User related
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<EmailAddress> UserEmails { get; set; } = null!;
    public DbSet<PhoneNumber> UserPhoneNumbers { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;

    // Account related
    public DbSet<Account> Accounts { get; set; } = null!;

    // Organization related
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationRole> OrganizationRoles { get; set; } = null!;

    // Developer and Application related
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationAccount> ApplicationAccounts { get; set; } = null!;

    // Token
    public DbSet<Token> Tokens { get; set; } = null!;

    // Notifications
    public DbSet<Notification> Notifications { get; set; } = null!;

    // Verification related
    public DbSet<VerificationSession> VerificationSessions { get; set; } = null!;
    public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Apply the converter to all DateTime properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }

        // Auto-configure Snowflake IDs for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null && idProperty.ClrType == typeof(long))
            {
                idProperty.ValueGenerated = ValueGenerated.Never;
            }
        }

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            // Indexes for performance
            entity.HasIndex(u => u.Username).IsUnique();

            // Cascade delete for user-owned entities
            entity.HasMany(u => u.EmailAddresses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasMany(u => u.PhoneNumbers)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom column types and names
            entity.Property(u => u.Username).HasColumnType("varchar(50)").HasColumnName("username");
            entity.Property(u => u.State).HasConversion<string>().HasMaxLength(20).HasColumnName("state");
            entity.Property(u => u.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(u => u.LastLoginAt).HasColumnType("timestamp").HasColumnName("last_login_at");
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
                .UsingEntity(j =>
                {
                    j.ToTable("account_developer_authorizations");
                    j.Property("AuthorizedAccountsId").HasColumnName("account_id");
                    j.Property("AuthorizedDevelopersId").HasColumnName("developer_id");
                });

            entity.HasMany(a => a.Roles)
                .WithMany(r => r.Members)
                .UsingEntity(j =>
                {
                    j.ToTable("account_organization_roles");
                    j.Property("MembersId").HasColumnName("account_id");
                    j.Property("RolesId").HasColumnName("role_id");
                });

            // Custom columns
            entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(a => a.IsPersonal).HasColumnName("is_personal");
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
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
            entity.Property(o => o.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
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
            entity.Property(d => d.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            entity.Property(d => d.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
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
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
        });

        // Token configurations
        modelBuilder.Entity<Token>(entity =>
        {
            entity.ToTable("tokens");

            // Indexes
            entity.HasIndex(t => t.TokenString).IsUnique();
            entity.HasIndex(t => t.ExpiresAt);
            entity.HasIndex(t => new { t.Target, t.UserId });
            entity.HasIndex(t => t.IsRevoked);

            // Relationships
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Session)
                .WithMany(s => s.Tokens)
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional relationships
            entity.HasOne(t => t.Account)
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.ApplicationAccount)
                .WithMany()
                .HasForeignKey(t => t.ApplicationAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Application)
                .WithMany()
                .HasForeignKey(t => t.ApplicationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.ParentToken)
                .WithMany()
                .HasForeignKey(t => t.ParentTokenId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Custom columns
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(t => t.Target).HasConversion<string>().HasMaxLength(20).HasColumnName("target");
            entity.Property(t => t.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(t => t.ExpiresAt).HasColumnType("timestamp").HasColumnName("expires_at");
        });

        // Session configurations
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");

            // Indexes
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.IsRevoked);
            entity.HasIndex(s => new { s.Target, s.UserId });
            entity.HasIndex(s => new { s.AccountId, s.ApplicationId });

            // Relationships
            entity.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Account)
                .WithMany(a => a.Sessions)
                .HasForeignKey(s => s.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.Application)
                .WithMany(a => a.Sessions)
                .HasForeignKey(s => s.ApplicationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Add column configuration
            entity.Property(s => s.IsRevoked)
                .HasDefaultValue(false)
                .HasColumnName("is_revoked");

            // Custom columns
            entity.Property(s => s.Target).HasConversion<string>().HasMaxLength(20).HasColumnName("target");
            entity.Property(s => s.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(s => s.LastUsedAt).HasColumnType("timestamp").HasColumnName("last_used_at");

            entity.HasMany(s => s.Tokens)
                .WithOne(t => t.Session)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(s => s.Id)
                .HasForeignKey(t => t.SessionId);
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            // Indexes
            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.HasIndex(n => new { n.AccountId, n.ApplicationId });

            // Relationships
            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Account)
                .WithMany()
                .HasForeignKey(n => n.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(n => n.Application)
                .WithMany()
                .HasForeignKey(n => n.ApplicationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Custom columns
            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(n => n.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(n => n.ReadAt).HasColumnType("timestamp").HasColumnName("read_at");
        });

        // ApplicationAccount configurations
        modelBuilder.Entity<ApplicationAccount>(entity =>
        {
            entity.ToTable("application_accounts");

            // Indexes
            entity.HasIndex(aa => aa.AccountId);
            entity.HasIndex(aa => aa.ApplicationId);

            // Relationships
            entity.HasOne(aa => aa.Account)
                .WithMany(a => a.AuthorizedApplications)
                .HasForeignKey(aa => aa.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(aa => aa.Application)
                .WithMany(a => a.Accounts)
                .HasForeignKey(aa => aa.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrganizationRole configurations
        modelBuilder.Entity<OrganizationRole>(entity =>
        {
            entity.ToTable("organization_roles");

            // Indexes
            entity.HasIndex(r => r.OrganizationId);

            // Relationships
            entity.HasOne(r => r.Organization)
                .WithMany(o => o.Roles)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EmailAddress configurations
        modelBuilder.Entity<EmailAddress>(entity =>
        {
            entity.ToTable("user_emails");

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Type })
                .IsUnique()
                .HasFilter("type = 'Primary'");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.EmailAddresses)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserEmails_Users");

            // Custom columns
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(e => e.State).HasConversion<string>().HasMaxLength(20).HasColumnName("state");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
        });

        // PhoneNumber configurations
        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.ToTable("user_phone_numbers");

            // Indexes
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.Phone).IsUnique();
            entity.HasIndex(p => new { p.UserId, p.Type })
                .IsUnique()
                .HasFilter("type = 'Primary'");

            // Relationships
            entity.HasOne(p => p.User)
                .WithMany(u => u.PhoneNumbers)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom columns
            entity.Property(p => p.Phone).HasColumnName("phone");
            entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(20).HasColumnName("type");
            entity.Property(p => p.State).HasConversion<string>().HasMaxLength(20).HasColumnName("state");
            entity.Property(p => p.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
        });

        // VerificationSession configurations
        modelBuilder.Entity<VerificationSession>(entity =>
        {
            entity.ToTable("verification_sessions");

            // Indexes
            entity.HasIndex(vs => vs.EmailId);
            entity.HasIndex(vs => vs.Code).IsUnique();

            // Relationships
            entity.HasOne(vs => vs.Email)
                .WithMany()
                .HasForeignKey(vs => vs.EmailId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(vs => vs.Phone)
                .WithMany()
                .HasForeignKey(vs => vs.PhoneId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(vs => vs.User)
                .WithMany()
                .HasForeignKey(vs => vs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Custom columns
            entity.Property(vs => vs.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(vs => vs.IsUsed).HasDefaultValue(false).HasColumnName("is_used");
        });

        // ResetPasswordRequest configurations
        modelBuilder.Entity<ResetPasswordRequest>(entity =>
        {
            entity.ToTable("reset_password_requests");

            // Relationships
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.EmailAddress)
                .WithMany()
                .HasForeignKey(r => r.EmailId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.EmailId);
            entity.HasIndex(r => r.CodeHash);

            // Column configurations
            entity.Property(r => r.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            entity.Property(r => r.ExpiredAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("expired_at");

            entity.Property(r => r.CodeHash).HasColumnName("code_hash");
        });
    }
}