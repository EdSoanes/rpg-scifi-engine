using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Sigill.Common.Authentication
{
    public class ClaimsPrincipalMiddleware : IFunctionsWorkerMiddleware
    {
        private class ClientPrincipal
        {
            public string? IdentityProvider { get; set; }
            public string? UserId { get; set; }
            public string? UserDetails { get; set; }
            public IEnumerable<string>? UserRoles { get; set; }
        }

        private readonly ILogger<ClaimsPrincipalMiddleware> _logger;

        public ClaimsPrincipalMiddleware(ILogger<ClaimsPrincipalMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequest = context.GetHttpContext()?.Request;
            var principal = GetClaimsPrincipal(httpRequest);
            var accessor = context.InstanceServices.GetRequiredService<IClaimsPrincipalAccessor>();
            accessor.Principal = principal;

            await next(context);
        }

        private ClaimsPrincipal GetClaimsPrincipal(HttpRequest? request)
        {
            var identity = request?.HttpContext?.User?.Identity ?? ClaimsPrincipal.Current?.Identity;
            if (identity != null && identity.IsAuthenticated)
                _logger.LogInformation("Found identity in httpcontext");
            else
            {
                identity = GetIdentityFromClientPrincipal(request);
                if (identity != null && identity.IsAuthenticated)
                    _logger.LogInformation("Found identity in header 'x-ms-client-principal'");
                else
                {
                    identity = GetIdentityFromJwtToken(request);
                    if (identity != null && identity.IsAuthenticated)
                        _logger.LogInformation("Found identity in header 'Authorization'");

                }
            }

            if (identity == null)
            {
                _logger.LogInformation("No identity found");
                return new ClaimsPrincipal();
            }

            return new ClaimsPrincipal(identity);
        }

        private ClaimsIdentity? GetIdentityFromClientPrincipal(HttpRequest? req)
        {
            if (req == null || !req.Headers.TryGetValue("x-ms-client-principal", out var header))
                return null;

            var data = header.First();
            if (string.IsNullOrWhiteSpace(data))
            {
                _logger.LogError("Invalid header 'x-ms-client-principal'");
                return null;
            }

            try
            {
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);

                var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                if (clientPrincipal == null || string.IsNullOrEmpty(clientPrincipal.UserId) || string.IsNullOrEmpty(clientPrincipal.UserDetails))
                {
                    _logger.LogError("Invalid header user 'x-ms-client-principal'");
                    return null;
                }

                var roles = clientPrincipal.UserRoles = clientPrincipal.UserRoles
                  ?.Except(new[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase)
                  ?? Enumerable.Empty<string>();

                var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, clientPrincipal.UserId));
                identity.AddClaim(new Claim(ClaimTypes.Name, clientPrincipal.UserDetails));
                identity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                return identity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading header 'x-ms-client-principal'");
                return null;
            }
        }

        private ClaimsIdentity? GetIdentityFromJwtToken(HttpRequest? req)
        {
            if (req == null || !req.Headers.TryGetValue("Authorization", out var header))
            {
                _logger.LogError("No Authorization jwt token found on request");
                return null;
            }

            var jwt = !string.IsNullOrEmpty(header.FirstOrDefault())
              ? req.Headers["Authorization"].ToString().Replace("Bearer", "").Trim()
              : null;

            if (string.IsNullOrWhiteSpace(jwt))
            {
                _logger.LogError("Invalid Authorization jwt token found on request");
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                var nameClaim = token.Claims?.FirstOrDefault(x => x.Type == "name");

                var identity = new ClaimsIdentity(token.Issuer);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, token.Subject));
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim?.Value ?? "Unknown"));
                identity.AddClaims(token.Claims ?? Enumerable.Empty<Claim?>());

                return identity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not deserialize jwt token found on request");
                return null;
            }
        }
    }
}
