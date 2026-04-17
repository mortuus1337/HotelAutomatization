using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hotel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    role_code = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    guest_id = table.Column<int>(type: "integer", nullable: false)
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
                name: "meal_plan",
                columns: table => new
                {
                    meal_plan_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    price_per_person_per_day = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plan", x => x.meal_plan_id);
                });

            migrationBuilder.CreateTable(
                name: "room_type",
                columns: table => new
                {
                    room_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    base_price = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_type", x => x.room_type_id);
                });

            migrationBuilder.CreateTable(
                name: "guest_identity",
                columns: table => new
                {
                    guest_id = table.Column<int>(type: "integer", nullable: false),
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
                name: "reservation",
                columns: table => new
                {
                    reservation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true),
                    customer_name = table.Column<string>(type: "text", nullable: true),
                    customer_phone = table.Column<string>(type: "text", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    planned_checkin = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_checkout = table.Column<DateOnly>(type: "date", nullable: false),
                    adults = table.Column<int>(type: "integer", nullable: false),
                    children = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric", nullable: true),
                    prepayment = table.Column<decimal>(type: "numeric", nullable: true),
                    meal_plan_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation", x => x.reservation_id);
                    table.ForeignKey(
                        name: "FK_reservation_app_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_meal_plan_meal_plan_id",
                        column: x => x.meal_plan_id,
                        principalTable: "meal_plan",
                        principalColumn: "meal_plan_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "room",
                columns: table => new
                {
                    room_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_number = table.Column<string>(type: "text", nullable: false),
                    room_type_id = table.Column<int>(type: "integer", nullable: false),
                    floor = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "reservation_room",
                columns: table => new
                {
                    reservation_room_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<int>(type: "integer", nullable: false),
                    room_id = table.Column<int>(type: "integer", nullable: false),
                    price_per_night = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_room", x => x.reservation_room_id);
                    table.ForeignKey(
                        name: "FK_reservation_room_reservation_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservation",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservation_room_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stay",
                columns: table => new
                {
                    stay_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<int>(type: "integer", nullable: true),
                    room_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    actual_checkin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actual_checkout = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    planned_checkin = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_checkout = table.Column<DateOnly>(type: "date", nullable: false),
                    meal_plan_id = table.Column<int>(type: "integer", nullable: true),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stay", x => x.stay_id);
                    table.ForeignKey(
                        name: "FK_stay_app_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stay_meal_plan_meal_plan_id",
                        column: x => x.meal_plan_id,
                        principalTable: "meal_plan",
                        principalColumn: "meal_plan_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stay_reservation_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservation",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stay_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stay_guest",
                columns: table => new
                {
                    stay_id = table.Column<int>(type: "integer", nullable: false),
                    guest_id = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stay_guest", x => new { x.stay_id, x.guest_id });
                    table.ForeignKey(
                        name: "FK_stay_guest_guest_guest_id",
                        column: x => x.guest_id,
                        principalTable: "guest",
                        principalColumn: "guest_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stay_guest_stay_stay_id",
                        column: x => x.stay_id,
                        principalTable: "stay",
                        principalColumn: "stay_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_user_login",
                table: "app_user",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_created_by_user_id",
                table: "reservation",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_meal_plan_id",
                table: "reservation",
                column: "meal_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_room_reservation_id",
                table: "reservation_room",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_room_room_id",
                table: "reservation_room",
                column: "room_id");

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
                name: "IX_stay_created_by_user_id",
                table: "stay",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_meal_plan_id",
                table: "stay",
                column: "meal_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_reservation_id",
                table: "stay",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_room_id",
                table: "stay",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_guest_guest_id",
                table: "stay_guest",
                column: "guest_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guest_identity");

            migrationBuilder.DropTable(
                name: "reservation_room");

            migrationBuilder.DropTable(
                name: "stay_guest");

            migrationBuilder.DropTable(
                name: "guest");

            migrationBuilder.DropTable(
                name: "stay");

            migrationBuilder.DropTable(
                name: "reservation");

            migrationBuilder.DropTable(
                name: "room");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "meal_plan");

            migrationBuilder.DropTable(
                name: "room_type");
        }
    }
}
