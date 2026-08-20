using backend.Data;
using backend.DTOs;
using backend.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace backend.Endpoints;
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users", async (ApplicationDbContext db, HttpContext httpContext) =>
{
    var currentUser = await httpContext.GetCurrentUser(db);

    if (currentUser is null || currentUser.Status == "blocked")
    {
        await httpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return Results.Unauthorized();
    }

    var users = await db.Users
        .OrderByDescending(u => u.LastLoginAt)
        .Select(u => new UserDto(
            u.Id,
            u.Name,
            u.Email,
            u.Status,
            u.RegisteredAt,
            u.LastLoginAt
        ))
        .ToListAsync();

    return Results.Ok(users);
})
    .RequireAuthorization();

    app.MapPost("/api/users/block",
    async (
        [FromBody] UserIdsRequest request,
        ApplicationDbContext db,
        HttpContext httpContext
    ) =>
    {
        var currentUser = await httpContext.GetCurrentUser(db);

        if (currentUser is null || currentUser.Status == "blocked")
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Unauthorized();
        }
        if (request.UserIds.Count == 0)
            return Results.BadRequest("No users selected");

        var users = await db.Users
            .Where(u => request.UserIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count == 0)
        {
            return Results.NotFound("Users not found");
        }

        var invalidUsers = users
            .Where(u => u.Status != "active")
            .ToList();

        if (invalidUsers.Count > 0)
        {
            return Results.BadRequest(
                "Only active users can be blocked"
            );
        }

        foreach (var user in users)
        {
            user.Status = "blocked";
        }

        await db.SaveChangesAsync();

        if (request.UserIds.Contains(currentUser.Id))
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Ok(new
            {
                message = "Current user blocked",
                redirectToLogin = true
            });
        }

        return Results.Ok(new
        {
            message = "Users blocked"
        });

    })
    .RequireAuthorization();

    app.MapPost("/api/users/unblock",
    async (
        [FromBody] UserIdsRequest request,
        ApplicationDbContext db,
        HttpContext httpContext
    ) =>
    {
        var currentUser = await httpContext.GetCurrentUser(db);

        if (currentUser is null || currentUser.Status == "blocked")
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Unauthorized();
        }
        if (request.UserIds.Count == 0)
            return Results.BadRequest("No users selected");

        var users = await db.Users
            .Where(u => request.UserIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count == 0)
        {
            return Results.NotFound("Users not found");
        }

        var invalidUsers = users
            .Where(u => u.Status != "blocked")
            .ToList();

        if (invalidUsers.Count > 0)
        {
            return Results.BadRequest(
                "Only blocked users can be unblocked"
            );
        }

        foreach (var user in users)
        {
            user.Status = "active";
        }

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Users unblocked"
        });
    })
    .RequireAuthorization();

    app.MapDelete("/api/users",
    async (
        [FromBody] UserIdsRequest request,
        ApplicationDbContext db,
        HttpContext httpContext
    ) =>
    {
        var currentUser = await httpContext.GetCurrentUser(db);

        if (currentUser is null || currentUser.Status == "blocked")
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Unauthorized();
        }

        if (request.UserIds.Count == 0)
            return Results.BadRequest("No users selected");

        var users = await db.Users
            .Where(u => request.UserIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count == 0)
            return Results.NotFound("Users not found");

        db.Users.RemoveRange(users);

        await db.SaveChangesAsync();

        if (request.UserIds.Contains(currentUser.Id))
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Ok(new
            {
                message = "Users deleted",
                redirectToLogin = true
            });
        }

        return Results.Ok(new
        {
            message = "Users deleted"
        });
    })
    .RequireAuthorization();
    }
}