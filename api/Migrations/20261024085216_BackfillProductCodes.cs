using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneSource.Api.Migrations
{
    /// <inheritdoc />
    public partial class BackfillProductCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH max_existing_code AS (
                    SELECT COALESCE(
                        MAX("ProductCode"::numeric) FILTER (WHERE "ProductCode" ~ '^[0-9]+$'),
                        0::numeric
                    ) AS value
                    FROM "Products"
                ),
                missing_codes AS (
                    SELECT
                        "ProductId",
                        ROW_NUMBER() OVER (ORDER BY "ProductId") AS row_number
                    FROM "Products"
                    WHERE "ProductCode" IS NULL OR BTRIM("ProductCode") = ''
                )
                UPDATE "Products" AS product
                SET "ProductCode" = LPAD((max_existing_code.value + missing_codes.row_number)::text, 3, '0')
                FROM max_existing_code, missing_codes
                WHERE product."ProductId" = missing_codes."ProductId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
