using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OAuth2Homework;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<Login> Login { get; set; }
    public DbSet<NotifyMessage> Messages { get; set; }
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

public class NotifyMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}
