using Microsoft.IdentityModel.Abstractions;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Sigill.Common.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal? principal)
          => principal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        public static string? GetEmail(this ClaimsPrincipal? principal)
        {
            var emails = principal?.Claims?.FirstOrDefault(x => x.Type == "emails")?.Value;
            if (!string.IsNullOrEmpty(emails))
            {
                var res = emails
                    .Trim('[', ']')
                    .Split(',')
                    .Select(x => x.Trim())
                    .FirstOrDefault();

                return res;
            }

            return null;
        }

        public static string[] GetScopes(this ClaimsPrincipal? principal)
        {
            var scp = principal?.Claims?.FirstOrDefault(x => x.Type == "scp")?.Value;
            return !string.IsNullOrEmpty(scp)
              ? scp.Split(',').Select(x => x.Trim()).ToArray()
              : new string[0];
        }

        public static string? GetName(this ClaimsPrincipal? principal)
          => principal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        public static string[] Roles(this ClaimsPrincipal? principal)
          => principal?.Claims
            ?.Where(x => x.Type == ClaimTypes.Role)
            ?.Select(x => x.Value)
            ?.ToArray()
            ?? new string[0];

        public static bool IsValidEmail(this ClaimsPrincipal? principal)
        {
            var email = principal.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }

                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsAuthenticated(this ClaimsPrincipal? principal)
          => principal?.Identity?.IsAuthenticated ?? false;
    }
}
