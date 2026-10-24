using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OneSource.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCarouselAndEnquiryPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnquiryWhatsAppNumber",
                table: "Contacts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnquiry",
                table: "ContactDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CarouselSlides",
                columns: table => new
                {
                    CarouselSlideId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AltText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselSlides", x => x.CarouselSlideId);
                });

            migrationBuilder.Sql("""
                UPDATE "Contacts"
                SET "EnquiryWhatsAppNumber" = "WhatsApp";

                UPDATE "ContactDetails"
                SET "IsEnquiry" = TRUE
                WHERE "Type" = 'whatsapp';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarouselSlides");

            migrationBuilder.DropColumn(
                name: "EnquiryWhatsAppNumber",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IsEnquiry",
                table: "ContactDetails");
        }
    }
}
