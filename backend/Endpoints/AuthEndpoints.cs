using backend.Models;
using backend.DTOs;
using backend.Data;
using backend.Services;
using Npgsql;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register",
    async (
        RegisterRequest request,
        ApplicationDbContext db,
        IPasswordHasher<User> hasher,
        IEmailQueue emailQueue,
        IConfiguration configuration
    ) =>
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("All fields are required");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Status = "unverified",
            RegisteredAt = DateTime.UtcNow,
            ConfirmationToken = Guid.NewGuid()
        };

        user.PasswordHash = hasher.HashPassword(
            user,
            request.Password
        );

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                pg.SqlState == "23505")
        {
            return Results.Conflict("Email already exists");
        }

        var frontendUrl = configuration["FrontendUrl"]!.TrimEnd('/');

        var confirmationLink = $"{frontendUrl}/confirm?token={user.ConfirmationToken}";

        await emailQueue.EnqueueAsync(
            new EmailMessage(
            user.Email,
            confirmationLink
            )
        );

        return Results.Ok(new
        {
            message = "Registration successful"
        });
    }
    );

        app.MapGet("/api/auth/confirm",
        async (
            Guid token,
            ApplicationDbContext db
        ) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.ConfirmationToken == token);

            if (user is null)
                return Results.NotFound("Invalid confirmation token");

            if (user.Status == "unverified")
            {
                user.Status = "active";
                user.ConfirmationToken = null;

                await db.SaveChangesAsync();
            }

            return Results.Ok("Email confirmed");
        }
        );

        app.MapPost("/api/auth/login",
        async (
            LoginRequest request,
            ApplicationDbContext db,
            IPasswordHasher<User> hasher,
            HttpContext httpContext
        ) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
                return Results.Json(
                    new { message = "Incorrect email or password" },
                    statusCode: StatusCodes.Status401Unauthorized
                );

            var passwordResult = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (passwordResult == PasswordVerificationResult.Failed)
                return Results.Json(
                    new { message = "Incorrect email or password" },
                    statusCode: StatusCodes.Status401Unauthorized
                );

            if (user.Status == "blocked")
                return Results.BadRequest("User is blocked");

            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var claims = new List<Claim>
            {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return Results.Ok(new
            {
                message = "Login successful"
            }
            );
        }
        );

        app.MapPost("/api/auth/logout",
        async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Results.Ok(new
            {
                message = "Logout successful"
            });
        })
        .RequireAuthorization();
    }
}