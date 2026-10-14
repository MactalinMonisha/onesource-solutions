namespace OneSource.Api.DTOs;

public record TrustedCompanyDto(
    int TrustedCompanyId,
    string CompanyName,
    string? LogoUrl,
    bool IsActive
);

public record CreateTrustedCompanyDto(
    string CompanyName
);

public record UpdateTrustedCompanyDto(
    string CompanyName
);
