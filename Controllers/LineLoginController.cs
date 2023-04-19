using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OAuth2Homework.Models;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace OAuth2Homework.Controllers;

public class LineLoginController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MyDbContext _dbContext;
    private readonly string _clientId, _clientSecret, _redirectUrl;

    public LineLoginController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, MyDbContext dbContext, IOptions<AppSettings> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _clientId = options.Value.LineLogin.ClientId;
        _clientSecret = options.Value.LineLogin.ClientSecret;
        _redirectUrl = options.Value.LineLogin.RedirectUrl;
    }

    public async Task<IActionResult> Callback(string code, string state)
    {
        // Exchange authorization code for access token
        HttpClient client = _httpClientFactory.CreateClient();
        Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUrl },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };
        FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
        HttpResponseMessage response = await client.PostAsync("https://api.line.me/oauth2/v2.1/token", content);
        string json = await response.Content.ReadAsStringAsync();
        AccessTokenResponse accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(json);


        // Get user profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);
        response = await client.GetAsync("https://api.line.me/v2/profile");
        json = await response.Content.ReadAsStringAsync();
        UserProfile userProfile = JsonSerializer.Deserialize<UserProfile>(json);

        var entity = new Login
        {
            UserId = userProfile.UserId,
            DisplayName = userProfile.DisplayName,
            AccessToken = accessTokenResponse.AccessToken,
            IdToken = accessTokenResponse.IdToken,
        };

        _dbContext.Login.Add(entity);
        await _dbContext.SaveChangesAsync();



        // Store user profile in session or database, and redirect to home page
        return RedirectToAction("Index", "LineLogin");
    }

    public IActionResult Index()
    {
        var login = _dbContext.Login.FirstOrDefault();
        if(login is null) RedirectToAction("Index", "Home");
        var vm = new SubscriptNotifyViewModel
        {
            WelcomeMessage = $"歡迎 {login.DisplayName}",
            HasSubscript = !string.IsNullOrEmpty(login.NotifyToken)
        };
        return View(vm);
    }

    public IActionResult Login()
    {
        if (_dbContext.Login.Any())
            return RedirectToAction("Index", "LineLogin");

        var authUrl = "https://access.line.me/oauth2/v2.1/authorize";
        var responseType = "code";
        var state = "123123";
        var scope = "openid email profile";

        var queryString = QueryString.Create(new Dictionary<string, string?>
        {
            {"response_type", responseType},
            {"client_id", _clientId},
            {"state", state},
            {"scope", scope},
            {"redirect_uri", _redirectUrl}
        });

        var uriBuilder = new UriBuilder(authUrl);
        uriBuilder.Query = queryString.ToString();

        return Redirect(uriBuilder.Uri.ToString());
    }

    public async Task<IActionResult> Logout()
    {
        var token = _dbContext.Login.FirstOrDefault();
        
        if(token is null) return RedirectToAction("Index", "Home");

        bool revokeResult = await RevokeToken(token.AccessToken);

        if (revokeResult)
        {
            _dbContext.Login.RemoveRange(_dbContext.Login);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index", "LineLogin");
    }

    private async Task<bool> RevokeToken(string? token)
    {
        var requestUri = "https://api.line.me/oauth2/v2.1/revoke";

        var values = new Dictionary<string, string>
        {
            {"access_token", token}
        };

        var content = new FormUrlEncodedContent(values);
        var httpClient = _httpClientFactory.CreateClient();
        var basicTokenString = $"{_clientId}:{_clientSecret}";
        var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicTokenString));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
        var response = await httpClient.PostAsync(requestUri, content);

        return response.IsSuccessStatusCode;
    }
}

public class SubscriptNotifyViewModel
{
    public string? WelcomeMessage { get; set; }
    public bool HasSubscript { get; set; }
}

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
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
    public string? UserId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("pictureUrl")]
    public string? PictureUrl { get; set; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; set; }
}