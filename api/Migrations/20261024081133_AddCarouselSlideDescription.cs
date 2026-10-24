using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneSource.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCarouselSlideDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CarouselSlides",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "CarouselSlides"
                SET "Description" = CASE "SortOrder"
                    WHEN 0 THEN 'Dependable technology and infrastructure to keep your business connected.'
                    WHEN 1 THEN 'Business-ready hardware and systems selected to support your day-to-day work.'
                    WHEN 2 THEN 'Practical guidance and ongoing support from setup through everyday use.'
                    ELSE "Title"
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CarouselSlides");
        }
    }
}
