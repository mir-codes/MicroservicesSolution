using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace User.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var tokenEndpoint = $"{keycloakConfig["Authority"]}/protocol/openid-connect/token";

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", keycloakConfig["ClientId"]!),
                new KeyValuePair<string, string>("client_secret", keycloakConfig["ClientSecret"]!),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password)
            });

                var response = await client.PostAsync(tokenEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                    return Ok(new { success = true, data = tokenResponse });
                }

                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { success = false, message = "Login failed" });
            }
        }
    }

    public record LoginRequest(string Username, string Password);
    public record TokenResponse(string access_token, int expires_in, string refresh_token, string token_type);
}
