using API.Infrastructure.Database.Entities.Account;
using API.Infrastructure.Database.Entities.Application;
using API.Infrastructure.Database.Entities.Authentication;
using API.Infrastructure.Database.Entities.Developer;
using API.Infrastructure.Database.Entities.Organization;
using API.Infrastructure.Database.Entities.User;
using API.Infrastructure.Database.Entities.Verification;
using API.Shared.Enums.Entities.Account;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.User;

public interface IGenerate
{
    User NewUser(long? id = null, string? username = null, string? firstName = null, string? lastName = null, string? passwordHash = null, string? passwordSalt = null, DateTime? birthDate = null, UserGender? gender = null, UserState? state = null);
    Account NewAccount(long? id = null, string? name = null, long? userId = null, AccountType? type = null);
    Application NewApplication(long? id = null, string? name = null, string? description = null, string? redirectUri = null, long? developerId = null, Developer? developer = null);
    Session NewSession(long? id = null, long? userId = null, SessionTarget? target = null, User? user = null, bool? isRevoked = null);
    Developer NewDeveloper(long? id = null, string? name = null, string? contactEmail = null, long? organizationId = null);
    Organization NewOrganization(long? id = null, string? name = null, string? description = null);
    ApplicationAccount NewApplicationAccount(long? id = null, long? applicationId = null, long? accountId = null);
    Token NewToken(long? id = null, string? value = null, long? sessionId = null, TokenType? type = null, TokenTarget? target = null, bool? isRevoked = null, User? user = null, Session? session = null);
    DeveloperAccount NewDeveloperAccount(long? id = null, string? name = null, long? developerId = null, long? accountId = null, Account? account = null);
    OrganizationRole NewOrganizationRole(long? id = null, string? name = null, string? description = null, long? organizationId = null, Organization? organization = null);
    EmailAddress NewEmailAddress(long? id = null, string? value = null, long? userId = null, EmailType? type = null, EmailState? state = null, User? user = null);
    PhoneNumber NewPhoneNumber(long? id = null, string? value = null, long? userId = null, PhoneType? type = null, PhoneState? state = null, User? user = null);
    EmailVerificationRequest NewEmailVerificationRequest(long? id = null, string? code = null, long? userId = null, long? emailId = null, EmailAddress? emailAddress = null, User? user = null, DateTime? expiresAt = null, bool? isUsed = null, bool? isRevoked = null);
    PasswordResetRequest NewPasswordResetRequest(long? id = null, string? codeHash = null, long? userId = null, long? emailId = null, EmailAddress? emailAddress = null, User? user = null, DateTime? expiresAt = null, bool? isUsed = null, bool? isRevoked = null);
}
