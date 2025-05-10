using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriPlat.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNutritionPlanAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserNutritionPlans",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    NutritionPlanId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedByNutritionistId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNutritionPlans", x => new { x.ClientId, x.NutritionPlanId });
                    table.ForeignKey(
                        name: "FK_UserNutritionPlans_AspNetUsers_AssignedByNutritionistId",
                        column: x => x.AssignedByNutritionistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNutritionPlans_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNutritionPlans_NutritionPlans_NutritionPlanId",
                        column: x => x.NutritionPlanId,
                        principalTable: "NutritionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserNutritionPlans_AssignedByNutritionistId",
                table: "UserNutritionPlans",
                column: "AssignedByNutritionistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNutritionPlans_NutritionPlanId",
                table: "UserNutritionPlans",
                column: "NutritionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNutritionPlans");
        }
    }
}
