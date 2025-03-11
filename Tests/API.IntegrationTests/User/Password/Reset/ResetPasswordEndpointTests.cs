using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.IntegrationTests.User.Password.Reset;
using API.Shared.Contracts.Responses.Common;
using NUnit.Framework;

namespace API.IntegrationTests.User.Password.Reset;

[TestFixture]
public class ResetPasswordEndpointTests : ResetPasswordTestBase
{
    [Test]
    public async Task ResetPassword_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/user/password/reset");
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
            "ResetPassword endpoint should return Method Not Allowed for GET requests");
    }

    [Test]
    public async Task ResetPassword_ValidRequest_ShouldReturnSuccess()
    {
        var (code, email) = await CreatePasswordResetRequestAsync();
        var newPassword = "NewPassword123!";
        var username = _testUsername;

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };
        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            "ResetPassword with valid request should return OK");

        var content = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        Assert.IsNotNull(content, "Response content should not be null");
        Assert.AreEqual("SUCCESS", content!.Status, "Response status should be SUCCESS");
    }

    [Test]
    public async Task ResetPassword_ShouldAllowLoginWithNewPassword()
    {
        var (code, email) = await CreatePasswordResetRequestAsync();
        var newPassword = "NewPassword123!";
        var username = _testUsername;

        var resetService = GetRequiredService<API.Features.User.Password.Reset.Interfaces.IResetPasswordService>();
        var resetResult = await resetService.ResetPasswordAsync(code, newPassword, CancellationToken.None);
        
        Assert.IsTrue(resetResult.IsSuccess, $"Password reset should succeed. Error: {resetResult.Message}");
        Assert.AreEqual(200, resetResult.HttpStatusCode, "Password reset should return HTTP 200");
        
        var loginService = GetRequiredService<API.Features.Authentication.Login.Interfaces.ILoginService>();
        var loginResult = await loginService.LoginAsync(username, newPassword, CancellationToken.None);
        
        Assert.IsTrue(loginResult.IsSuccess, 
            $"Login with new password should succeed. Error: {loginResult.Message}");
        Assert.AreEqual(200, loginResult.HttpStatusCode, "Login should return HTTP 200");
    }

    [Test]
    public async Task ResetPassword_InvalidCode_ShouldReturnNotFound()
    {
        var invalidCode = "invalid-reset-code-format-9876543210";
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = invalidCode, NewPassword = newPassword };
        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with invalid code should return NotFound");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_ExpiredCode_ShouldReturnBadRequest()
    {
        var (code, _) = await CreateExpiredPasswordResetRequestAsync();
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };
        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with expired code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_UsedCode_ShouldReturnBadRequest()
    {
        var (code, _) = await CreateAndUsePasswordResetRequestAsync();
        var newPassword = "NewTestPassword123!";

        var request = new ResetPasswordRequest { Code = code, NewPassword = newPassword };
        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
            "ResetPassword with used code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
        Assert.That(errorContent.Status, Is.EqualTo("ERROR").IgnoreCase);
    }

    [Test]
    public async Task ResetPassword_MissingCode_ShouldReturnBadRequest()
    {
        var newPassword = "NewTestPassword123!";
        var request = new ResetPasswordRequest { Code = null!, NewPassword = newPassword };

        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with missing code should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }

    [Test]
    public async Task ResetPassword_MissingNewPassword_ShouldReturnBadRequest()
    {
        var (code, _) = await CreatePasswordResetRequestAsync();
        var request = new ResetPasswordRequest { Code = code, NewPassword = null! };

        var response = await PostAsync("/api/user/password/reset", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "ResetPassword with missing new password should return BadRequest");

        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent, "Error response should not be null");
        Assert.IsNotNull(errorContent!.Message, "Error response should contain a message");
    }
}