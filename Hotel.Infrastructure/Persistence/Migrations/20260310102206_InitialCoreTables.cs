using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hotel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    role_code = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "guest",
                columns: table => new
                {
                    guest_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guest", x => x.guest_id);
                });

            migrationBuilder.CreateTable(
                name: "room_type",
                columns: table => new
                {
                    room_type_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_type", x => x.room_type_id);
                    table.CheckConstraint("ck_room_type_base_price_non_negative", "\"base_price\" >= 0");
                    table.CheckConstraint("ck_room_type_capacity_positive", "\"capacity\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "guest_identity",
                columns: table => new
                {
                    guest_id = table.Column<long>(type: "bigint", nullable: false),
                    doc_type = table.Column<string>(type: "text", nullable: false),
                    doc_number = table.Column<string>(type: "text", nullable: false),
                    issued_by = table.Column<string>(type: "text", nullable: true),
                    issued_date = table.Column<DateOnly>(type: "date", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    citizenship = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guest_identity", x => x.guest_id);
                    table.ForeignKey(
                        name: "FK_guest_identity_guest_guest_id",
                        column: x => x.guest_id,
                        principalTable: "guest",
                        principalColumn: "guest_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room",
                columns: table => new
                {
                    room_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_number = table.Column<string>(type: "text", nullable: false),
                    room_type_id = table.Column<long>(type: "bigint", nullable: false),
                    floor = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room", x => x.room_id);
                    table.ForeignKey(
                        name: "FK_room_room_type_room_type_id",
                        column: x => x.room_type_id,
                        principalTable: "room_type",
                        principalColumn: "room_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_user_login",
                table: "app_user",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_room_number",
                table: "room",
                column: "room_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_room_type_id",
                table: "room",
                column: "room_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_type_name",
                table: "room_type",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "guest_identity");

            migrationBuilder.DropTable(
                name: "room");

            migrationBuilder.DropTable(
                name: "guest");

            migrationBuilder.DropTable(
                name: "room_type");
        }
    }
}
