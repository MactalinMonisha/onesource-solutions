namespace OneSource.Api.DTOs;

public record ContactDto(
    int ContactId,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Location,
    string? Website
);

public record UpdateContactDto(
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Location,
    string? Website
);
