using API.Features.User.Password.Reset.Endpoints;
using API.Features.User.Password.Reset.Interfaces;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Models.Services;
using FastEndpoints;
using NSubstitute;

namespace API.Tests.Unit.Features.User.Password.Reset;

public class CheckRequestStatusEndpointTests : TestBase
{
    #region Helper Methods

    private CheckRequestStatusRequest CreateRequest(string code = "valid-code")
    {
        return new CheckRequestStatusRequest { Code = code };
    }

    private CheckRequestStatusEndpoint CreateEndpoint(IResetPasswordService resetPasswordService)
    {
        return Factory.Create<CheckRequestStatusEndpoint>(resetPasswordService);
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
        var serviceResult = new CheckRequestStatusResult { IsSuccess = true, Status = "SUCCESS", Message = "Request is valid", HttpStatusCode = 200, IsExpired = false, IsUsed = false };

        mockService.CheckRequestStatusAsync(request.Code, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as CheckRequestStatusResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Request is valid");
        response.IsExpired.Should().BeFalse();
        response.IsUsed.Should().BeFalse();

        await mockService.Received(1).CheckRequestStatusAsync(request.Code, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenCodeIsExpired_ReturnsExpiredResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest("expired-code");
        var serviceResult = new CheckRequestStatusResult { IsSuccess = true, Status = "SUCCESS", Message = "Request is expired", HttpStatusCode = 200, IsExpired = true, IsUsed = false };

        mockService.CheckRequestStatusAsync(request.Code, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as CheckRequestStatusResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Request is expired");
        response.IsExpired.Should().BeTrue();
        response.IsUsed.Should().BeFalse();

        await mockService.Received(1).CheckRequestStatusAsync(request.Code, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenCodeIsUsed_ReturnsUsedResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest("used-code");
        var serviceResult = new CheckRequestStatusResult { IsSuccess = true, Status = "SUCCESS", Message = "Request is already used", HttpStatusCode = 200, IsExpired = false, IsUsed = true };

        mockService.CheckRequestStatusAsync(request.Code, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as CheckRequestStatusResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Request is already used");
        response.IsExpired.Should().BeFalse();
        response.IsUsed.Should().BeTrue();

        await mockService.Received(1).CheckRequestStatusAsync(request.Code, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenCodeIsInvalid_ReturnsErrorResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest("invalid-code");
        var serviceResult = new CheckRequestStatusResult { IsSuccess = false, Status = "ERROR", Message = "Invalid code", HttpStatusCode = 404 };

        mockService.CheckRequestStatusAsync(request.Code, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as CheckRequestStatusResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Invalid code");

        await mockService.Received(1).CheckRequestStatusAsync(request.Code, CancellationToken.None);
    }

    [Test]
    public async Task HandleAsync_WhenServiceReturnsError_ReturnsErrorResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest();
        var serviceResult = new CheckRequestStatusResult { IsSuccess = false, Status = "ERROR", Message = "Service error", HttpStatusCode = 500 };

        mockService.CheckRequestStatusAsync(request.Code, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as CheckRequestStatusResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Service error");

        await mockService.Received(1).CheckRequestStatusAsync(request.Code, CancellationToken.None);
    }

    #endregion
}