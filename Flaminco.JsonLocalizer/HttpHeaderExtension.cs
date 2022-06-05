using Microsoft.AspNetCore.Http;

namespace Flaminco.JsonLocalizer;

internal static class HttpHeaderExtension
{
    internal static string GetAcceptLanguage(this IHttpContextAccessor _accessor, string defaultLanguage = "en-US")
    {
        string? userLangs = _accessor?.HttpContext?.Request.Headers["Accept-Language"].ToString();
        string? firstLang = userLangs?.Split(',').FirstOrDefault();
        return string.IsNullOrEmpty(firstLang) ? defaultLanguage : firstLang;
    }

}
