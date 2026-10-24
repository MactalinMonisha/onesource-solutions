using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OneSource.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicContactDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactDetails",
                columns: table => new
                {
                    ContactDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetails", x => x.ContactDetailId);
                    table.ForeignKey(
                        name: "FK_ContactDetails_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetails_ContactId",
                table: "ContactDetails",
                column: "ContactId");

            migrationBuilder.Sql("""
                INSERT INTO "ContactDetails" ("ContactId", "Label", "Value", "Type", "SortOrder", "CreatedDate", "UpdatedDate")
                SELECT c."ContactId", details."Label", details."Value", details."Type", details."SortOrder", CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                FROM "Contacts" c
                CROSS JOIN LATERAL (VALUES
                    ('Phone', c."Phone", 'phone', 0),
                    ('WhatsApp', c."WhatsApp", 'whatsapp', 1),
                    ('Email', c."Email", 'email', 2),
                    ('Location', c."Location", 'address', 3),
                    ('Website', c."Website", 'website', 4),
                    ('Postal Address', 'P.O. Box 221 – 00623, Nairobi, Kenya', 'text', 5)
                ) AS details("Label", "Value", "Type", "SortOrder")
                WHERE NULLIF(BTRIM(details."Value"), '') IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactDetails");
        }
    }
}
