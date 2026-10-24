namespace OneSource.Api.DTOs;

public record CarouselSlideDto(
    int CarouselSlideId,
    string ImageUrl,
    string AltText,
    string Label,
    string Description,
    string Title,
    int SortOrder
);

public record UpdateCarouselSlideDto(
    string ImageUrl,
    string AltText,
    string Label,
    string Description,
    string Title
);
