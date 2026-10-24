namespace OneSource.Api.DTOs;

public record ProductDto(
    int ProductId,
    string ProductName,
    string? ProductDescription,
    string? ProductCode,
    int CatId,
    string? CategoryName,
    bool IsActive
);

public record ProductImageDto(
    int ProductImageId,
    string ImageUrl,
    bool IsActive
);

public record ProductWithImagesDto(
    int ProductId,
    string ProductName,
    string? ProductDescription,
    string? ProductCode,
    int CatId,
    string? CategoryName,
    bool IsActive,
    IEnumerable<ProductImageDto> ProductImages
);

public record CreateProductDto(
    string ProductName,
    string? ProductDescription,
    int CatId,
    string? ProductCode = null
);

public record CreateProductWithImagesDto(
    string ProductName,
    string? ProductDescription,
    int CatId,
    List<string> ImageUrls,
    string? ProductCode = null
);

public record UploadProductImagesDto(
    List<string> ImageUrls
);

public record UpdateProductDto(
    string ProductName,
    string? ProductDescription,
    int CatId,
    bool IsActive,
    string? ProductCode = null
);

public record ProductImportResultDto(int Created, int Updated, int CategoriesCreated);
