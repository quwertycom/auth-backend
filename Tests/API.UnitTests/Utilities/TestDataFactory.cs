
using API.Models;


namespace API.UnitTests.Utilities;

public static class TestDataFactory
{
    public static User CreateValidUser() => new User {
        Id = 1,
        Username = "testuser",
        FirstName = "Test",
        LastName = "User",
        PasswordHash = "hash",
        PasswordSalt = "salt",
        BirthDate = DateTime.UtcNow.AddYears(-20)
    };
}