using backend.Data;
using backend.Models;
using Npgsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using backend.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "task4_auth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    }
);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

static async Task<User?> GetCurrentUser(
    HttpContext httpContext,
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

app.MapGet("/users", async (ApplicationDbContext db, HttpContext httpContext) =>
{
    var currentUser = await GetCurrentUser(httpContext, db);

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

app.MapPost("/api/auth/register",
    async (
        RegisterRequest request,
        ApplicationDbContext db,
        IPasswordHasher<User> hasher
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
            return Results.Unauthorized();

        var passwordResult = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (passwordResult == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        if (user.Status == "unverified")
            return Results.BadRequest("Email is not confirmed");

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

app.MapPost("/api/users/block",
    async (
        [FromBody] UserIdsRequest request,
        ApplicationDbContext db,
        HttpContext httpContext
    ) =>
    {
        var currentUser = await GetCurrentUser(httpContext, db);

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
        var currentUser = await GetCurrentUser(httpContext, db);

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
        var currentUser = await GetCurrentUser(httpContext, db);

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

// app.MapPost("/test-user", async (ApplicationDbContext db) =>
// {
//     var user = new User
//     {
//         Name = "Ansar",
//         Email = "ansar@test1.com",
//         PasswordHash = "test",
//         Status = "unverified",
//         ConfirmationToken = Guid.NewGuid()
//     };

//     try
//     {
//         db.Users.Add(user);
//         await db.SaveChangesAsync();

//         return Results.Ok(user);
//     }
//     catch (DbUpdateException ex)
//         when (ex.InnerException is PostgresException pg &&
//             pg.SqlState == "23505")
//     {
//         return Results.Conflict("Email already exists");
//     }    
// });

app.Run();