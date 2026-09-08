namespace OneSource.Api.DTOs;

public record LoginDto(string Username, string Password);

public record LoginResultDto(string Token, string Username, string Role, DateTime ExpiresAt);

public record RegisterDto(string Username, string MailId, string Password, string Role);

public record UserDto(int UserId, string Username, string MailId, string Role);
