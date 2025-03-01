using FastEndpoints;

namespace API.Features.Authentication.Register.Endpoints;

public class RegisterEndpoint : EndpointWithoutRequest<object>
{
  public override void Configure()
  {
    Post("/api/authentication/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var response = new { message = "hello world" };
    await SendOkAsync(response, ct);
  }
}
