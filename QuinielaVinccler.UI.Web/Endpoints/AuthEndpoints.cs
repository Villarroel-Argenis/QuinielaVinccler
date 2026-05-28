
using Microsoft.AspNetCore.Mvc;

namespace QuinielaVinccler.UI.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/signin", async (
       HttpContext ctx,
       PendingLoginService pending,
       [FromForm] string token,
       [FromForm] string? returnUrl) =>
        {
            var principal = pending.Consume(token);

            if (principal is null)
                return Results.Redirect("/login?error=expired");

            await ctx.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            var destination = IsLocalUrl(returnUrl) ? returnUrl! : "/mis-planillas";
            return Results.Redirect(destination);
        })
        .AllowAnonymous()
        .DisableAntiforgery();

        app.MapPost("/api/auth/signout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        })
        .AllowAnonymous(); // ← también signout, por si el token expira
    }

    private static bool IsLocalUrl(string? url) =>
        !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//");
}