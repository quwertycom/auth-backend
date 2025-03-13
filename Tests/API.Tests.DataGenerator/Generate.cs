using API.Infrastructure.Database.Entities.Account;
using API.Infrastructure.Database.Entities.Application;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.Developer;
using API.Infrastructure.Database.Entities.Organization;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.Account;
using API.Shared.Enums.Entities.Application;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.Developer;
using API.Shared.Enums.Entities.User;
using API.Shared.Utilities;

namespace API.Tests.DataGenerator;

public class Generate : IGenerate
{
    public User NewUser(long? id = null, string? username = null, string? firstName = null, string? lastName = null, string? passwordHash = null, string? passwordSalt = null, DateTime? birthDate = null, UserGender? gender = null, UserState? state = null)
    {
        return new User
        {
            Id = id ?? Snowflake.Generate(),
            Username = username ?? "testuser",
            FirstName = firstName ?? "Test",
            LastName = lastName ?? "User",
            PasswordHash = passwordHash ?? "password",
            PasswordSalt = passwordSalt ?? "salt",
            BirthDate = birthDate ?? new DateTime(1990, 1, 1),
            Gender = gender ?? UserGender.Male,
            State = state ?? UserState.Active
        };
    }

    public Account NewAccount(long? id = null, string? name = null, long? userId = null, AccountType? type = null)
    {
        return new Account
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test Account",
            UserId = userId ?? 1,
            Type = type ?? AccountType.Personal,
            User = NewUser()
        };
    }

    public Application NewApplication(long? id = null, string? name = null, string? description = null, string? redirectUri = null, long? developerId = null, Developer? developer = null)
    {
        return new Application
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test App",
            Description = description ?? "Test",
            Developer = developer ?? NewDeveloper(developerId ?? 1),
            RedirectUri = redirectUri ?? "https://test.com",
            DeveloperId = developerId ?? 1,
            Status = ApplicationStatus.Development
        };
    }

    public Session NewSession(long? id = null, long? userId = null, SessionTarget? target = null)
    {
        return new Session
        {
            Id = id ?? Snowflake.Generate(),
            UserId = userId ?? 1,
            Target = target ?? SessionTarget.User,
            User = NewUser()
        };
    }

    public Developer NewDeveloper(long? id = null, string? name = null, string? contactEmail = null, long? organizationId = null)
    {
        return new Developer
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test Dev",
            ContactEmail = contactEmail ?? "test@dev.com",
            OrganizationId = organizationId ?? 1,
            Organization = NewOrganization(organizationId ?? 1),
            Type = DeveloperType.Personal,
            Status = DeveloperStatus.NotVerified
        };
    }

    public Organization NewOrganization(long? id = null, string? name = null, string? description = null)
    {
        return new Organization
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test Org",
            Description = description ?? "Test"
        };
    }

    public ApplicationAccount NewApplicationAccount(long? id = null, long? applicationId = null, long? accountId = null)
    {
        return new ApplicationAccount
        {
            Id = id ?? Snowflake.Generate(),
            ApplicationId = applicationId ?? 1,
            AccountId = accountId ?? 1,
            Sessions = new List<Session>(),
            Application = NewApplication(),
            Account = NewAccount()
        };
    }

    public Token NewToken(long? id = null, string? value = null, long? sessionId = null, TokenType? type = null, TokenTarget? target = null)
    {
        return new Token
        {
            Id = id ?? Snowflake.Generate(),
            Value = value ?? "test",
            SessionId = sessionId ?? 1,
            Type = type ?? TokenType.Access,
            Target = target ?? TokenTarget.User,
            Session = NewSession(),
            User = NewUser()
        };
    }

    public DeveloperAccount NewDeveloperAccount(long? id = null, string? name = null, long? developerId = null, long? accountId = null, Account? account = null)
    {
        return new DeveloperAccount
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test Dev Acc",
            DeveloperId = developerId ?? 1,
            Developer = NewDeveloper(),
            AccountId = accountId ?? 1,
            Account = account ?? NewAccount(),
            Status = DeveloperStatus.NotVerified,
            Type = DeveloperType.Personal
        };
    }

    public OrganizationRole NewOrganizationRole(long? id = null, string? name = null, string? description = null, long? organizationId = null, Organization? organization = null)
    {
        return new OrganizationRole
        {
            Id = id ?? Snowflake.Generate(),
            Name = name ?? "Test Role",
            Description = description ?? "Test",
            OrganizationId = organizationId ?? 1,
            Organization = organization ?? NewOrganization(organizationId ?? 1)
        };
    }

    public EmailAddress NewEmailAddress(long? id = null, string? value = null, long? userId = null, EmailType? type = null, EmailState? state = null, User? user = null)
    {   
        return new EmailAddress
        {
            Id = id ?? Snowflake.Generate(),
            Value = value ?? "test@test.com",
            UserId = userId ?? 1,
            Type = type ?? EmailType.Primary,
            State = state ?? EmailState.PendingVerification,
            User = user ?? NewUser()
        };
    }

    public PhoneNumber NewPhoneNumber(long? id = null, string? value = null, long? userId = null, PhoneType? type = null, PhoneState? state = null, User? user = null)
    {
        return new PhoneNumber
        {
            Id = id ?? Snowflake.Generate(),
            Value = value ?? "+123456789",
            UserId = userId ?? 1,
            Type = type ?? PhoneType.Primary,
            State = state ?? PhoneState.PendingVerification,
            User = user ?? NewUser()
        };
    }

    public EmailVerificationRequest NewEmailVerificationRequest(long? id = null, string? code = null, long? userId = null, long? emailId = null, EmailAddress? emailAddress = null, User? user = null)
    {
        return new EmailVerificationRequest
        {
            Id = id ?? Snowflake.Generate(),
            Code = code ?? "123456",
            UserId = userId ?? 1,
            EmailId = emailId ?? 1,
            User = user ?? NewUser(),
            EmailAddress = emailAddress ?? NewEmailAddress()
        };
    }

    public PasswordResetRequest NewPasswordResetRequest(long? id = null, string? codeHash = null, long? userId = null, long? emailId = null, EmailAddress? emailAddress = null, User? user = null)
    {
        return new PasswordResetRequest
        {
            Id = id ?? Snowflake.Generate(),
            CodeHash = codeHash ?? "hash",
            UserId = userId ?? 1,
            EmailId = emailId ?? 1,
            User = user ?? NewUser(),
            EmailAddress = emailAddress ?? NewEmailAddress()
        };
    }
}
