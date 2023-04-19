using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OAuth2Homework.Controllers;

public class LineNotifyController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MyDbContext _dbContext;
    private readonly string _clientId, _clientSecret, _redirectUrl;

    public LineNotifyController(IHttpClientFactory httpClientFactory, MyDbContext dbContext, IOptions<AppSettings> options)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _clientId = options.Value.LineNotify.ClientId;
        _clientSecret = options.Value.LineNotify.ClientSecret;
        _redirectUrl = options.Value.LineNotify.RedirectUrl;
    }

    public IActionResult Subscript()
    {
        return Redirect($"https://notify-bot.line.me/oauth/authorize?response_type=code&client_id={_clientId}&state=123123&scope=notify&redirect_uri={_redirectUrl}");
    }

    // public async Task<IActionResult> Callback(string code, string state)
    // {
    //     // Exchange authorization code for access token
    //     HttpClient client = _httpClientFactory.CreateClient();
    //     Dictionary<string, string> parameters = new Dictionary<string, string>
    //         {
    //             { "grant_type", "authorization_code" },
    //             { "code", code },
    //             { "redirect_uri", _redirectUrl },
    //             { "client_id", _clientId },
    //             { "client_secret", _clientSecret }
    //         };
    //     FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
    //     HttpResponseMessage response = await client.PostAsync("https://api.line.me/oauth2/v2.1/token", content);
    //     string json = await response.Content.ReadAsStringAsync();
    //     AccessTokenResponse accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(json);

    //     _dbContext.AccessTokens.Add(accessTokenResponse);

    //     // Get user profile
    //     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);
    //     response = await client.GetAsync("https://api.line.me/v2/profile");
    //     json = await response.Content.ReadAsStringAsync();
    //     UserProfile userProfile = JsonSerializer.Deserialize<UserProfile>(json);
    //     _dbContext.UserProfiles.Add(userProfile);

    //     ViewBag.WelcomeMessage = $"歡迎 {userProfile.DisplayName}";

    //     await _dbContext.SaveChangesAsync();

    //     // Store user profile in session or database, and redirect to home page
    //     return RedirectToAction("SubscriptNotify", "Home");
    // }
}