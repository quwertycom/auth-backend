

using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //TODO: Add Models
}