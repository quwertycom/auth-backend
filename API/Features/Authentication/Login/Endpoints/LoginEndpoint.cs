using FastEndpoints;
using API.Features.Authentication.Login.Models.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using API.Features.Authentication.Login.Interfaces;
namespace API.Features.Authentication.Login.Endpoints;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly ILoginService _loginService;

    public LoginEndpoint(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public override void Configure()
    {
        Post("/api/authentication/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Login a user";
            s.Description = "Logs in a user with the provided credentials";
            s.Response(200, "User login successful");
            s.Response(400, "Invalid request data");
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _loginService.LoginAsync(req.Username, req.Password, ct);

        await SendAsync(new LoginResponse
        {
            Status = result.Status,
            Message = result.Message,
            RefreshToken = result.RefreshToken,
            AccessToken = result.AccessToken
        }, statusCode: result.HttpStatusCode ?? (result.IsSuccess ? 200 : 400), ct);
    }
}