using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Auth.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddKeycloakAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakSection = configuration.GetSection("Keycloak");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = keycloakSection["Authority"];
                    options.Audience = keycloakSection["Audience"];
                    options.RequireHttpsMetadata = keycloakSection.GetValue<bool>("RequireHttpsMetadata", false);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudiences = new[]
                        {
                        keycloakSection["Audience"]!,
                        "account"
                        },
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Extract roles from realm_access
                            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                            if (claimsIdentity != null)
                            {
                                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                                if (!string.IsNullOrEmpty(realmAccess))
                                {
                                    try
                                    {
                                        var roles = JsonDocument.Parse(realmAccess)
                                            .RootElement.GetProperty("roles").EnumerateArray()
                                            .Select(r => r.GetString() ?? string.Empty)
                                            .Where(r => !string.IsNullOrEmpty(r));

                                        foreach (var role in roles)
                                        {
                                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error parsing realm_access: {ex.Message}");
                                    }
                                }

                                // Extract client roles if needed
                                var resourceAccess = context.Principal?.FindFirst("resource_access")?.Value;
                                if (!string.IsNullOrEmpty(resourceAccess))
                                {
                                    try
                                    {
                                        var clientId = keycloakSection["Audience"];
                                        var doc = JsonDocument.Parse(resourceAccess);

                                        if (doc.RootElement.TryGetProperty(clientId!, out var clientAccess))
                                        {
                                            if (clientAccess.TryGetProperty("roles", out var clientRoles))
                                            {
                                                var roles = clientRoles.EnumerateArray()
                                                    .Select(r => r.GetString() ?? string.Empty)
                                                    .Where(r => !string.IsNullOrEmpty(r));

                                                foreach (var role in roles)
                                                {
                                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, $"client:{role}"));
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error parsing resource_access: {ex.Message}");
                                    }
                                }
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                // Default policy requires authentication
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }

        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ??
                   user.FindFirst("preferred_username")?.Value ??
                   user.FindFirst("email")?.Value;
        }

        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   user.FindFirst("sub")?.Value;
        }

        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }

        public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
        }
    }
}
