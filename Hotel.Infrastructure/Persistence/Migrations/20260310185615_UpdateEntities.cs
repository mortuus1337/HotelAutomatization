using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hotel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meal_plan",
                columns: table => new
                {
                    meal_plan_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    price_per_person_per_day = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plan", x => x.meal_plan_id);
                    table.CheckConstraint("ck_meal_plan_price_positive", "\"price_per_person_per_day\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "reservation",
                columns: table => new
                {
                    reservation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    planned_checkin = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_checkout = table.Column<DateOnly>(type: "date", nullable: false),
                    adults = table.Column<int>(type: "integer", nullable: false),
                    children = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    prepayment = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    MealPlanId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation", x => x.reservation_id);
                    table.CheckConstraint("ck_reservation_adults", "\"adults\" >= 1");
                    table.CheckConstraint("ck_reservation_dates", "\"planned_checkout\" > \"planned_checkin\"");
                    table.ForeignKey(
                        name: "FK_reservation_app_user_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_reservation_meal_plan_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plan",
                        principalColumn: "meal_plan_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservation_room",
                columns: table => new
                {
                    reservation_room_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationId = table.Column<long>(type: "bigint", nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    price_per_night = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_room", x => x.reservation_room_id);
                    table.ForeignKey(
                        name: "FK_reservation_room_reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "reservation",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservation_room_room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stay",
                columns: table => new
                {
                    stay_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationId = table.Column<long>(type: "bigint", nullable: true),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    ActualCheckin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualCheckout = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    planned_checkin = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_checkout = table.Column<DateOnly>(type: "date", nullable: false),
                    MealPlanId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stay", x => x.stay_id);
                    table.CheckConstraint("ck_stay_dates", "\"planned_checkout\" > \"planned_checkin\"");
                    table.ForeignKey(
                        name: "FK_stay_app_user_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_stay_meal_plan_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plan",
                        principalColumn: "meal_plan_id");
                    table.ForeignKey(
                        name: "FK_stay_reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "reservation",
                        principalColumn: "reservation_id");
                    table.ForeignKey(
                        name: "FK_stay_room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stay_guest",
                columns: table => new
                {
                    stay_id = table.Column<long>(type: "bigint", nullable: false),
                    guest_id = table.Column<long>(type: "bigint", nullable: false),
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
                name: "IX_meal_plan_name",
                table: "meal_plan",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_CreatedByUserId",
                table: "reservation",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_MealPlanId",
                table: "reservation",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_room_ReservationId",
                table: "reservation_room",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_room_RoomId",
                table: "reservation_room",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_stay_CreatedByUserId",
                table: "stay",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_stay_MealPlanId",
                table: "stay",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_stay_ReservationId",
                table: "stay",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_stay_RoomId",
                table: "stay",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_stay_guest_guest_id",
                table: "stay_guest",
                column: "guest_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_room");

            migrationBuilder.DropTable(
                name: "stay_guest");

            migrationBuilder.DropTable(
                name: "stay");

            migrationBuilder.DropTable(
                name: "reservation");

            migrationBuilder.DropTable(
                name: "meal_plan");
        }
    }
}
