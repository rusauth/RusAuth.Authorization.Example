namespace RusAuth.Authorization.Example.Infrastructure;

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Services;

public sealed class CallbackBearerAuthorizationFilter(IOptions<ExampleCallbackOptions> callbackOptions)
    : IAuthorizationFilter
{
    private readonly ExampleCallbackOptions _callbackOptions = callbackOptions.Value;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authorizationHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (TryGetBearerToken(authorizationHeader, out var providedToken) is false ||
            FixedTimeEquals(providedToken, _callbackOptions.CallbackBearerToken) is false)
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private static bool TryGetBearerToken(string authorizationHeader, out string token)
    {
        token = string.Empty;

        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return false;

        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase) is false)
            return false;

        token = authorizationHeader[bearerPrefix.Length..].Trim();
        return string.IsNullOrWhiteSpace(token) is false;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length &&
               CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}