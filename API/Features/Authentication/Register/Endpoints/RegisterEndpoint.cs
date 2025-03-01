using FastEndpoints;
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Interfaces;

namespace API.Features.Authentication.Register.Endpoints;

public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
  private readonly IRegisterService _registerService;

  public RegisterEndpoint(IRegisterService registerService)
  {
    _registerService = registerService;
  }

  public override void Configure()
  {
    Post("/api/authentication/register");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Register a new user";
      s.Description = "Creates a new user account with the provided details";
      s.Response(200, "User registration successful");
      s.Response(400, "Invalid request data");
    });
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    var result = await _registerService.RegisterUserAsync(
      req.Username, 
      req.Password, 
      ct
    );

    await SendOkAsync(new RegisterResponse { Status = "success", Message = "User registered successfully", EmailVerificationSessionId = "123" }, ct);
  }
}