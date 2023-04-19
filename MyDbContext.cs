using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OAuth2Homework;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<Login> Login { get; set; }
}

public class Login
{
    [Key]
    public string? UserId { get; internal set; }
    public string? DisplayName { get; internal set; }
    public string? AccessToken { get; set; }
    public string? IdToken { get; set; }
    public string? NotifyToken { get; set; }
}
