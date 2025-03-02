using FastEndpoints;
using API.Features.Authentication.Register.Models.Contracts;
using API.Features.Authentication.Register.Interfaces;
using API.Shared.Contracts.Responses.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ErrorResponse = API.Shared.Contracts.Responses.Common.ErrorResponse;

namespace API.Features.Authentication.Register.Endpoints;

public class RegisterEndpoint : Endpoint<RegisterRequest, Results<Ok<RegisterResponse>, BadRequest<ErrorResponse>>>
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

  public override async Task<Results<Ok<RegisterResponse>, BadRequest<ErrorResponse>>> ExecuteAsync(RegisterRequest req, CancellationToken ct)
  {
    var result = await _registerService.RegisterUserAsync(req, ct);

    Console.WriteLine(result.Status + " " + result.Message);

    if (result.IsSuccess && result.EmailVerificationSessionId != null) {
      return TypedResults.Ok(new RegisterResponse {
        Status = "SUCCESS",
        EmailVerificationSessionId = result.EmailVerificationSessionId
      });
    }
    
    ErrorResponse errorResponse;
    
    if (result.Status == "USERNAME_EXISTS") {
      errorResponse = new ErrorResponse {
        Status = "USERNAME_EXISTS",
        Message = "Username already exists",
        Details = new Dictionary<string, List<string>> {
          { "username", new List<string> { "Username already exists" } }
        },
        Timestamp = DateTime.UtcNow
      };
    }
    else if (result.Status == "EMAIL_EXISTS") {
      errorResponse = new ErrorResponse {
        Status = "EMAIL_EXISTS",
        Message = "Email already exists",
        Details = new Dictionary<string, List<string>> {
          { "email", new List<string> { "Email already exists" } }
        },
        Timestamp = DateTime.UtcNow
      };
    }
    else if (result.Status == "PHONE_NUMBER_EXISTS") {
      errorResponse = new ErrorResponse {
        Status = "PHONE_NUMBER_EXISTS",
        Message = "Phone number already exists",
        Details = new Dictionary<string, List<string>> {
          { "phoneNumber", new List<string> { "Phone number already exists" } }
        },
        Timestamp = DateTime.UtcNow
      };
    }
    else {
      errorResponse = new ErrorResponse {
        Status = "INTERNAL_SERVER_ERROR",
        Message = "Something went wrong, please try again later.",
        Details = new Dictionary<string, List<string>>(),
        Timestamp = DateTime.UtcNow
      };
    }
    
    return TypedResults.BadRequest(errorResponse);
  }
}