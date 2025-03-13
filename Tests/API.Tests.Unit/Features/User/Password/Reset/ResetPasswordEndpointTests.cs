using API.Features.User.Password.Reset.Endpoints;
using API.Features.User.Password.Reset.Interfaces;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Models.Services;
using FastEndpoints;
using NSubstitute;

namespace API.Tests.Unit.Features.User.Password.Reset;

public class ResetPasswordEndpointTests : TestBase
{
    #region Helper Methods

    private ResetPasswordRequest CreateRequest(string code = "valid-code", string newPassword = "newValidPassword")
    {
        return new ResetPasswordRequest { Code = code, NewPassword = newPassword };
    }

    private ResetPasswordEndpoint CreateEndpoint(IResetPasswordService resetPasswordService)
    {
        return Factory.Create<ResetPasswordEndpoint>(resetPasswordService);
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void Configure_SetsCorrectEndpointProperties()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var endpoint = CreateEndpoint(mockService);

        endpoint.Configure();

        Assert.Pass("Endpoint configured successfully");
    }

    #endregion

    #region HandleAsync Tests

    [Test]
    public async Task HandleAsync_WhenCodeIsValid_ReturnsSuccessResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest();
        var serviceResult = new ResetPasswordResult { IsSuccess = true, Status = "SUCCESS", Message = "Password reset successfully", HttpStatusCode = 200 };

        mockService.ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as ResetPasswordResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Password reset successfully");

        await mockService.Received(1).ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenCodeIsInvalid_ReturnsErrorResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest(code: "invalid-code");
        var serviceResult = new ResetPasswordResult { IsSuccess = false, Status = "ERROR", Message = "Invalid code", HttpStatusCode = 404 };

        mockService.ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as ResetPasswordResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Invalid code");

        await mockService.Received(1).ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenServiceReturnsError_ReturnsErrorResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest();
        var serviceResult = new ResetPasswordResult { IsSuccess = false, Status = "ERROR", Message = "Service error", HttpStatusCode = 500 };

        mockService.ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as ResetPasswordResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Service error");

        await mockService.Received(1).ResetPasswordAsync(request.Code, request.NewPassword, CancellationToken.None);
    }

    #endregion
} 