namespace backend.DTOs;

public record RegisterRequest(
    string Name,
    string Email,
    string Password
);