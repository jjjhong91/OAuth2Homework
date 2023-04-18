using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OAuth2Homework.Models;

namespace OAuth2Homework.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string _redirectUrl = "https://localhost:7022/Home/LineLogin";
    private const string _clientId = "1660922220";
    private const string _clientSecret = "84a762d04dcd4ecbfea720f4bb3e9bf5";

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        TempData.Clear();
        return View();
    }

    public async Task<IActionResult> LineLogin()
    {

        if (Request.Query.TryGetValue("code", out var code))
        {
            var tokenResponse = await GetAccessToken(code);

            if (tokenResponse is not null)
            {
                TempData.Add("access_token", tokenResponse.AccessToken);
                TempData.Add("id_token", tokenResponse.IdToken);
            }

            return RedirectToAction(nameof(LineLogin));
        }

        var hasToken = TempData.TryGetValue("access_token", out var token);

        if (!hasToken)
            return Redirect(GetAuthUrl().ToString());

        return View();
    }

    public async Task<IActionResult> LineLogout()
    {
        var hasToken = TempData.TryGetValue("access_token", out var token);
        if (hasToken)
        {
            bool revokeResult = await RevokeToken(token.ToString());
            if (revokeResult)
            {
                TempData.Clear();
            }
        }
        return RedirectToAction(nameof(Index));
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
        var response = await httpClient.PostAsync(requestUri, content);

        return response.IsSuccessStatusCode;
    }

    private async Task<TokenResponse?> GetAccessToken(string code)
    {
        var requestUri = "https://api.line.me/oauth2/v2.1/token";

        var values = new Dictionary<string, string>
        {
            {"grant_type","authorization_code"},
            {"code",code},
            {"redirect_uri",_redirectUrl},
            {"client_id", _clientId},
            {"client_secret", _clientSecret},
            {"id_token_key_type", "JWK"},
        };
        var content = new FormUrlEncodedContent(values);
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(requestUri, content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(json);
        }

        return null;
    }

    private Uri GetAuthUrl()
    {
        var authUrl = "https://access.line.me//oauth2/v2.1/authorize";
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

        return uriBuilder.Uri;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public class TokenResponse
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
}
