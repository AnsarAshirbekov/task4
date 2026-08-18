namespace backend.DTOs;

public record UserDto(
    long Id,
    string Name,
    string Email,
    string Status,
    DateTime RegisteredAt,
    DateTime? LastLoginAt
);