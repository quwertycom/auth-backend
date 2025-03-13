using API.Features.Session.Revoke.Models.Services;
using API.Features.Session.Revoke.Services;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Enums.Entities.Authentication;
using API.Shared.Enums.Entities.User;

namespace API.Tests.Unit.Features.Session.Revoke;

public class RevokeSessionServiceTests : TestBase
{
    #region Helper Methods

    private API.Infrastructure.Database.Entities.Authentication.Session CreateMockSession(long id = 1, bool isRevoked = false)
    {
        return new API.Infrastructure.Database.Entities.Authentication.Session
        {
            Id = id,
            IsRevoked = isRevoked,
            UserId = 1,
            Target = SessionTarget.User,
            User = new API.Infrastructure.Database.Entities.User.User
            {
                Id = 1,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                Gender = UserGender.Male,
                State = UserState.Active
            }
        };
    }

    #endregion

    #region RevokeSessionAsync Tests

    [Test]
    public async Task RevokeSessionAsync_ValidSessionId_ReturnsSuccessResult()
    {
        // Arrange
        var mockSession = CreateMockSession(1);
        var mockSessionRepository = Substitute.For<ISessionRepository>();

        mockSessionRepository.GetSessionByIdAsync(1).Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(mockSession));
        mockSessionRepository.RevokeSessionAsync(1).Returns(Task.CompletedTask);

        var revokeSessionService = new RevokeSessionService(mockSessionRepository);

        // Act
        var result = await revokeSessionService.RevokeSessionAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("SUCCESS");
        result.Message.Should().Be("Session revoked");
        // HttpStatusCode is null in the success case and handled by controller (returns 200)

        await mockSessionRepository.Received(1).RevokeSessionAsync(1);
    }

    [Test]
    public async Task RevokeSessionAsync_SessionNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var mockSessionRepository = Substitute.For<ISessionRepository>();

        mockSessionRepository.GetSessionByIdAsync(1).Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(null));

        var revokeSessionService = new RevokeSessionService(mockSessionRepository);

        // Act
        var result = await revokeSessionService.RevokeSessionAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Be("Session not found");
        result.HttpStatusCode.Should().Be(404);

        await mockSessionRepository.DidNotReceive().RevokeSessionAsync(Arg.Any<long>());
    }

    [Test]
    public async Task RevokeSessionAsync_SessionAlreadyRevoked_ReturnsErrorResult()
    {
        // Arrange
        var mockSession = CreateMockSession(1, true);
        var mockSessionRepository = Substitute.For<ISessionRepository>();

        mockSessionRepository.GetSessionByIdAsync(1).Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(mockSession));

        var revokeSessionService = new RevokeSessionService(mockSessionRepository);

        // Act
        var result = await revokeSessionService.RevokeSessionAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Be("Session has been already revoked");
        result.HttpStatusCode.Should().Be(400);

        await mockSessionRepository.DidNotReceive().RevokeSessionAsync(Arg.Any<long>());
    }

    [Test]
    public async Task RevokeSessionAsync_ExceptionThrown_ReturnsErrorResult()
    {
        // Arrange
        var mockSession = CreateMockSession(1);
        var mockSessionRepository = Substitute.For<ISessionRepository>();

        mockSessionRepository.GetSessionByIdAsync(1).Returns(Task.FromResult<API.Infrastructure.Database.Entities.Authentication.Session?>(mockSession));
        mockSessionRepository.When(x => x.RevokeSessionAsync(1)).Do(x => { throw new Exception("Test exception"); });

        var revokeSessionService = new RevokeSessionService(mockSessionRepository);

        // Act
        var result = await revokeSessionService.RevokeSessionAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be("ERROR");
        result.Message.Should().Be("Test exception");
        result.HttpStatusCode.Should().Be(500);
    }

    #endregion
}