using System.Net;
using System.Net.Http.Json;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Shared.Contracts.Responses.Common;
using NUnit.Framework;

namespace API.Tests.Integration.User.Password.Reset;

[TestFixture]
public class CheckRequestStatusTests : ResetPasswordTestBase
{
    [Test]
    public async Task CheckRequestStatus_Endpoint_Should_BeAccessible()
    {
        var response = await _client.GetAsync("/api/user/password/reset/request-status");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task CheckRequestStatus_ValidCode_ShouldReturnSuccess()
    {
        var (code, _) = await CreatePasswordResetRequestAsync();
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content);
        Assert.AreEqual("SUCCESS", content!.Status);
        Assert.IsFalse(content.IsExpired);
        Assert.IsFalse(content.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatus_InvalidCode_ShouldReturnNotFound()
    {
        var invalidCode = "invalid-reset-code-format-1234567890";
        var response = await GetAsync($"/api/user/password/reset/request-status?code={invalidCode}");
        
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent);
        Assert.IsNotNull(errorContent!.Message);
    }

    [Test]
    public async Task CheckRequestStatus_ExpiredCode_ShouldReturnSuccessAndExpiredStatus()
    {
        var (code, _) = await CreateExpiredPasswordResetRequestAsync();
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content);
        Assert.AreEqual("SUCCESS", content!.Status);
        Assert.IsTrue(content.IsExpired);
        Assert.IsFalse(content.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatus_UsedCode_ShouldReturnSuccessAndUsedStatus()
    {
        var (code, _) = await CreateAndUsePasswordResetRequestAsync();
        var response = await GetAsync($"/api/user/password/reset/request-status?code={code}");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CheckRequestStatusResponse>();
        Assert.IsNotNull(content);
        Assert.AreEqual("SUCCESS", content!.Status);
        Assert.IsFalse(content.IsExpired);
        Assert.IsTrue(content.IsUsed);
    }

    [Test]
    public async Task CheckRequestStatus_MissingCode_ShouldReturnBadRequest()
    {
        var response = await GetAsync($"/api/user/password/reset/request-status?code=");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(errorContent);
        Assert.IsNotNull(errorContent!.Message);
    }
}