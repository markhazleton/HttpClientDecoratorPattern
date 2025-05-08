using WebSpark.Bootswatch.Model;
using Microsoft.AspNetCore.Http;
using HttpClientDecorator.Web.Helpers;
using System.Text.Json;

namespace HttpClientDecorator.Web.Pages.Shared;

public class ThemeSettings
{
    public string Name { get; set; } = "default";
    public string ColorMode { get; set; } = "light";
}

public static class ThemeHelper
{
    private const string SessionKeyTheme = "UserThemeSettings";

    public static ThemeSettings GetThemeSettings(HttpContext context)
    {
        // First try to get from session
        var sessionTheme = context.Session.GetObjectFromJson<ThemeSettings>(SessionKeyTheme);

        if (sessionTheme != null)
        {
            return sessionTheme;
        }

        // If not in session, try to get from query string
        var queryTheme = context.Request.Query["theme"].ToString();

        // Get color mode from cookie or default
        var colorModeCookie = context.Request.Cookies["color-mode"];
        var colorMode = !string.IsNullOrEmpty(colorModeCookie)
            ? colorModeCookie
            : (queryTheme?.Contains("dark") == true ? "dark" : "light");

        // Create new settings
        var themeSettings = new ThemeSettings
        {
            Name = !string.IsNullOrEmpty(queryTheme) ? queryTheme : "default",
            ColorMode = colorMode
        };

        // Save in session for future use
        context.Session.SetObjectAsJson(SessionKeyTheme, themeSettings);

        return themeSettings;
    }

    public static void SetTheme(HttpContext context, string themeName)
    {
        var currentSettings = GetThemeSettings(context);
        currentSettings.Name = themeName;
        context.Session.SetObjectAsJson(SessionKeyTheme, currentSettings);
    }

    public static void SetColorMode(HttpContext context, string colorMode)
    {
        var currentSettings = GetThemeSettings(context);
        currentSettings.ColorMode = colorMode;
        context.Session.SetObjectAsJson(SessionKeyTheme, currentSettings);
    }
}