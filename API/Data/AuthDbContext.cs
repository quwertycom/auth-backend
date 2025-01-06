using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // TODO: Add datasets
}