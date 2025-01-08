namespace API.Common.Enums;

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