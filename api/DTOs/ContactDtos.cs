namespace OneSource.Api.DTOs;

public record ContactDto(
    int ContactId,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Location,
    string? Website,
    string? EnquiryWhatsAppNumber,
    IReadOnlyList<ContactDetailDto> Details
);

public record UpdateContactDto(
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Location,
    string? Website,
    IReadOnlyList<UpdateContactDetailDto>? Details = null
);

public record ContactDetailDto(
    int ContactDetailId,
    string Label,
    string Value,
    string Type,
    int SortOrder,
    bool IsEnquiry
);

public record UpdateContactDetailDto(
    string Label,
    string Value,
    string Type,
    bool IsEnquiry = false
);
