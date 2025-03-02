
public record VerifyEmailRequest {
  public required string EmailVerificationSessionId { get; set; }
  public required string Code { get; set; }
}