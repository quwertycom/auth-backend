using API.Features.Authentication.Login.Models.Contracts;
using API.Shared.Enums.Entities.User;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Security;

namespace API.Tests.Integration.User.Password.Reset;

[TestFixture]
public abstract class ResetPasswordTestBase : TestBase
{
    #region Helper Methods

    protected string _testUsername = $"test-reset-{Guid.NewGuid()}";

    protected async Task<(string code, string email)> CreatePasswordResetRequestAsync()
    {
        var userRepository = GetRequiredService<IUserRepository>();
        var verificationRepository = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();
        var randomGenerator = GetRequiredService<IRandomGenerator>();

        _testUsername = $"test-reset-{Guid.NewGuid()}";
        var email = $"reset-{Guid.NewGuid()}@example.com";

        var newUser = _generate.NewUser(
            username: _testUsername, 
            passwordHash: hasher.Hash("Password123!").Hash,
            state: UserState.Active
        );
        await userRepository.AddUserAsync(newUser);

        var newEmail = _generate.NewEmailAddress(
            value: email, 
            user: newUser,
            state: EmailState.Active,
            type: EmailType.Primary
        );
        await userRepository.AddEmailAsync(newEmail);

        var code = randomGenerator.GenerateAlphanumericCode(64);
        var passwordResetRequest = _generate.NewPasswordResetRequest(
            codeHash: hasher.Hash(code, "").Hash,
            user: newUser,
            emailAddress: newEmail,
            expiresAt: DateTime.UtcNow.AddMinutes(10)
        );

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);
        return (code, email);
    }

    protected async Task<(string code, string email)> CreateExpiredPasswordResetRequestAsync()
    {
        var userRepository = GetRequiredService<IUserRepository>();
        var verificationRepository = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();
        var randomGenerator = GetRequiredService<IRandomGenerator>();

        var username = $"test-expired-reset-{Guid.NewGuid()}";
        var email = $"expired-reset-{Guid.NewGuid()}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        var user = await userRepository.GetUserByUsernameAsync(username);
        var emailAddress = user?.EmailAddresses.FirstOrDefault();

        var code = randomGenerator.GenerateAlphanumericCode(64);
        var passwordResetRequest = _generate.NewPasswordResetRequest(
            codeHash: hasher.Hash(code, "").Hash,
            user: user,
            emailAddress: emailAddress,
            expiresAt: DateTime.UtcNow.AddMinutes(-10)
        );

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);
        return (code, email);
    }

    protected async Task<(string code, string email)> CreateAndUsePasswordResetRequestAsync()
    {
        var userRepository = GetRequiredService<IUserRepository>();
        var verificationRepository = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();
        var randomGenerator = GetRequiredService<IRandomGenerator>();

        var username = $"test-used-reset-{Guid.NewGuid()}";
        var email = $"used-reset-{Guid.NewGuid()}@example.com";
        await EnsureVerifiedUserExistsAsync(username, "Password123!");

        var user = _generate.NewUser(
            username: username,
            passwordHash: hasher.Hash("Password123!").Hash,
            state: UserState.Active
        );
        await userRepository.AddUserAsync(user);

        var emailAddress = _generate.NewEmailAddress(
            value: email,
            user: user,
            state: EmailState.Active,
            type: EmailType.Primary
        );
        await userRepository.AddEmailAsync(emailAddress);

        var code = randomGenerator.GenerateAlphanumericCode(64);
        var passwordResetRequest = _generate.NewPasswordResetRequest(
            codeHash: hasher.Hash(code, "").Hash,
            user: user,
            emailAddress: emailAddress,
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            isUsed: true
        );

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);
        return (code, email);
    }

    protected async Task<HttpResponseMessage> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };
        return await PostAsync("/api/authentication/login", loginRequest);
    }

    protected async Task<(string code, string email)> CreatePasswordResetRequestAsync(string existingUsername, string existingEmail)
    {
        var userRepository = GetRequiredService<IUserRepository>();
        var verificationRepository = GetRequiredService<IVerificationRepository>();
        var hasher = GetRequiredService<IHasher>();
        var randomGenerator = GetRequiredService<IRandomGenerator>();

        var user = await userRepository.GetUserByUsernameAsync(existingUsername);
        var email = await userRepository.GetEmailAdressByEmailStringAsync(existingEmail);

        if (user == null || email == null)
        {
            throw new Exception("User or email address not found");
        }

        var code = randomGenerator.GenerateAlphanumericCode(64);
        var passwordResetRequest = _generate.NewPasswordResetRequest(
            codeHash: hasher.Hash(code, "").Hash,
            user: user,
            emailAddress: email,
            expiresAt: DateTime.UtcNow.AddMinutes(10)
        );

        await verificationRepository.AddPasswordResetRequestAsync(passwordResetRequest);
        return (code, existingEmail);
    }

    #endregion
}