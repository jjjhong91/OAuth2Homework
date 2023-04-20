using System.Net.Http.Headers;
using System.Text;
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

    public async Task<IActionResult> Unsubscript()
    {
        var login = _dbContext.Login.FirstOrDefault();
        
        if(login is null) return RedirectToAction("Index", "Home");
        if(string.IsNullOrEmpty(login.NotifyToken)) return RedirectToAction("Index", "LineLogin");

        var requestUri = "https://notify-api.line.me/api/revoke";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.NotifyToken);
        var response = await httpClient.PostAsync(requestUri, null);

        if (response.IsSuccessStatusCode)
        {
            login.NotifyToken = string.Empty;
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "LineLogin");
        }

        return RedirectToAction("Index", "LineLogin");
    }

    public async Task<IActionResult> Callback(string code, string state)
    {
        // Exchange authorization code for access token
        var client = _httpClientFactory.CreateClient();
        var parameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUrl },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };
        var content = new FormUrlEncodedContent(parameters);
        var response = await client.PostAsync("https://notify-bot.line.me/oauth/token", content);
        string json = await response.Content.ReadAsStringAsync();
        var accessTokenResponse = JsonSerializer.Deserialize<LineNotifyAccessTokenResponse>(json);

        var login = _dbContext.Login.FirstOrDefault();

       login.NotifyToken = accessTokenResponse.AccessToken;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction("Index", "LineLogin");
    }

    [HttpPost]
    public async Task<IActionResult> SubmitMessage(string message)
    {
        var login = _dbContext.Login.FirstOrDefault();
        var requestUri = "https://notify-api.line.me/api/notify";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.NotifyToken);
        var parameters = new Dictionary<string, string>
            {
                { "message", message },
            };
        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(requestUri, content);

        _dbContext.Messages.Add(new NotifyMessage { Message = message, Timestamp = DateTime.Now});
        await _dbContext.SaveChangesAsync();

        return RedirectToAction("Index", "LineLogin");
    }
}