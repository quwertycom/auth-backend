using API.Features.User.Password.Reset.Endpoints;
using API.Features.User.Password.Reset.Interfaces;
using API.Features.User.Password.Reset.Models.Contracts;
using API.Features.User.Password.Reset.Models.Services;
using FastEndpoints;
using NSubstitute;

namespace API.Tests.Unit.Features.User.Password.Reset;

public class RequestPasswordResetEndpointTests : TestBase
{
    #region Helper Methods

    private RequestPasswordResetRequest CreateRequest(string email = "test@example.com", string username = "")
    {
        return new RequestPasswordResetRequest { Email = email, Username = username };
    }

    private RequestPasswordResetEndpoint CreateEndpoint(IResetPasswordService resetPasswordService)
    {
        return Factory.Create<RequestPasswordResetEndpoint>(resetPasswordService);
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
    public async Task HandleAsync_WhenEmailProvided_CallsEmailServiceAndReturnsSuccess()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest();
        var serviceResult = new RequestPasswordResetResult { IsSuccess = true, Status = "SUCCESS", Message = "Reset request initiated", HttpStatusCode = 200 };

        mockService.RequestPasswordResetViaEmailAsync(request.Email!, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as RequestPasswordResetResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Reset request initiated");

        await mockService.Received(1).RequestPasswordResetViaEmailAsync(request.Email!, CancellationToken.None);
        await mockService.DidNotReceive().RequestPasswordResetViaUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WhenUsernameProvided_CallsUsernameServiceAndReturnsSuccess()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest(email: "", username: "testuser");
        var serviceResult = new RequestPasswordResetResult { IsSuccess = true, Status = "SUCCESS", Message = "Reset request initiated via username", HttpStatusCode = 200 };

        mockService.RequestPasswordResetViaUsernameAsync(request.Username!, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as RequestPasswordResetResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("SUCCESS");
        response.Message.Should().Be("Reset request initiated via username");

        await mockService.Received(1).RequestPasswordResetViaUsernameAsync(request.Username!, CancellationToken.None);
        await mockService.DidNotReceive().RequestPasswordResetViaEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WhenBothEmailAndUsernameProvided_PrioritizesEmail()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest(email: "test@example.com", username: "testuser");
        var serviceResult = new RequestPasswordResetResult { IsSuccess = true, Status = "SUCCESS", Message = "Reset request initiated via email", HttpStatusCode = 200 };

        mockService.RequestPasswordResetViaEmailAsync(request.Email!, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        await mockService.Received(1).RequestPasswordResetViaEmailAsync(request.Email!, CancellationToken.None);
        await mockService.DidNotReceive().RequestPasswordResetViaUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WhenServiceReturnsError_ReturnsErrorResponse()
    {
        var mockService = Substitute.For<IResetPasswordService>();
        var request = CreateRequest();
        var serviceResult = new RequestPasswordResetResult { IsSuccess = false, Status = "ERROR", Message = "Email not found", HttpStatusCode = 404 };

        mockService.RequestPasswordResetViaEmailAsync(request.Email!, CancellationToken.None).Returns(serviceResult);

        var endpoint = CreateEndpoint(mockService);

        await endpoint.HandleAsync(request, CancellationToken.None);

        var response = endpoint.Response as RequestPasswordResetResponse;
        Assert.NotNull(response);
        response!.Status.Should().Be("ERROR");
        response.Message.Should().Be("Email not found");
    }
    #endregion
}
