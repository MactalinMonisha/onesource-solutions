namespace OneSource.Api.DTOs;

public record TrustedCompanyDto(
    int TrustedCompanyId,
    string CompanyName,
    bool IsActive
);

public record CreateTrustedCompanyDto(
    string CompanyName
);

public record UpdateTrustedCompanyDto(
    string CompanyName
);
