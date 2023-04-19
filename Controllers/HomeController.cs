using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OAuth2Homework.Models;
using System.Net.Http.Headers;
using System.Text;

namespace OAuth2Homework.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MyDbContext _dbContext;
    private const string _clientId = "1660922220";
    private const string _clientSecret = "84a762d04dcd4ecbfea720f4bb3e9bf5";
    private const string _redirectUrl = "https://localhost:7022/Home/Callback";

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, MyDbContext dbContext)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return View();
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

        _dbContext.AccessTokens.Add(accessTokenResponse);

        // Get user profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);
        response = await client.GetAsync("https://api.line.me/v2/profile");
        json = await response.Content.ReadAsStringAsync();
        UserProfile userProfile = JsonSerializer.Deserialize<UserProfile>(json);
        _dbContext.UserProfiles.Add(userProfile);

        ViewBag.WelcomeMessage = $"歡迎 {userProfile.DisplayName}";

        await _dbContext.SaveChangesAsync();

        // Store user profile in session or database, and redirect to home page
        return RedirectToAction("SubscriptNotify", "Home");
    }

    public IActionResult SubscriptNotify()
    {
        var userProfile = _dbContext.UserProfiles.FirstOrDefault();
        var vm = new SubscriptNotifyViewModel
        {
            WelcomeMessage = $"歡迎 {userProfile.DisplayName}"
        };
        return View(vm);
    }

    public IActionResult Login()
    {
        if (_dbContext.AccessTokens.FirstOrDefault() != null)
            return RedirectToAction("SubscriptNotify", "Home");

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
        var token = _dbContext.AccessTokens.First();
        bool revokeResult = await RevokeToken(token.AccessToken);

        if (revokeResult)
        {
            _dbContext.AccessTokens.RemoveRange(_dbContext.AccessTokens);
            _dbContext.UserProfiles.RemoveRange(_dbContext.UserProfiles);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("SubscriptNotify", "Home");
    }

    private async Task<bool> RevokeToken(string token)
    {
        var requestUri = "https://api.line.me/oauth2/v2.1/revoke";

        var values = new Dictionary<string, string>
        {
            {"access_token",token}
        };

        var content = new FormUrlEncodedContent(values);
        var httpClient = _httpClientFactory.CreateClient();
        var basicTokenString = $"{_clientId}:{_clientSecret}";
        var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicTokenString));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
        var response = await httpClient.PostAsync(requestUri, content);

        return response.IsSuccessStatusCode;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class SubscriptNotifyViewModel
{
    public string WelcomeMessage { get; set; }
}
