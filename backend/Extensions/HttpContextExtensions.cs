using backend.Models;
using backend.Data;
using System.Security.Claims;

namespace backend.Extensions;
public static class HttpContextExtensions
{
    public static async Task<User?> GetCurrentUser(
    this HttpContext httpContext,
    ApplicationDbContext db)
    {
        var claim = httpContext.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (claim is null)
            return null;

        if (!long.TryParse(claim, out var userId))
            return null;

        return await db.Users.FindAsync(userId);
    }
}