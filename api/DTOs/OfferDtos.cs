namespace OneSource.Api.DTOs;

public record OfferDto(
    int OfferId,
    string OfferName,
    string? Description,
    bool IsActive
);

public record CreateOfferDto(
    string OfferName,
    string? Description
);

public record UpdateOfferDto(
    string OfferName,
    string? Description
);
