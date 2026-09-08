namespace OneSource.Api.DTOs;

public record ProductCategoryDto(
    int CateId,
    string CatName,
    string? Description,
    bool IsActive
);

public record CreateProductCategoryDto(
    string CatName,
    string? Description
);
