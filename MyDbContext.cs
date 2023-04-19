using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace OAuth2Homework;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<AccessTokenResponse> AccessTokens { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
}

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    [Key]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}

public class UserProfile
{
    [JsonPropertyName("userId")]
    [Key]
    public string? UserId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("pictureUrl")]
    public string? PictureUrl { get; set; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; set; }
}
