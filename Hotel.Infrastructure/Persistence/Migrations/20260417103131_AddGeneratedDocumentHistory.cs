using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hotel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedDocumentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "generated_document",
                columns: table => new
                {
                    generated_document_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    generated_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_document", x => x.generated_document_id);
                    table.ForeignKey(
                        name: "FK_generated_document_app_user_generated_by_user_id",
                        column: x => x.generated_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_generated_document_document_type_generated_at",
                table: "generated_document",
                columns: new[] { "document_type", "generated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_generated_document_entity_type_entity_id",
                table: "generated_document",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_generated_document_generated_at",
                table: "generated_document",
                column: "generated_at");

            migrationBuilder.CreateIndex(
                name: "IX_generated_document_generated_by_user_id",
                table: "generated_document",
                column: "generated_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "generated_document");
        }
    }
}
