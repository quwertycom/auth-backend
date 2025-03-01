namespace API.Shared.Enums.Entities.User;

public enum EmailState
{
    PendingVerification,
    Active,
    Blacklisted,
    Disabled,
    Deleted
}