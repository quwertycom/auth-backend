namespace API.Core.Enums;

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    StatusChanged,
    PermissionChanged,
    Authenticated,
    PasswordChanged,
    EmailVerified,
    TokenIssued,
    TokenRevoked
}