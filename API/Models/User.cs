public class User {
    public long Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}