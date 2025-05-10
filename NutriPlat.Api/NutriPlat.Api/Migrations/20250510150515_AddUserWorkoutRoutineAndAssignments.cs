using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriPlat.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWorkoutRoutineAndAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "NutritionPlans",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserWorkoutRoutines",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkoutRoutineId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedByTrainerId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkoutRoutines", x => new { x.ClientId, x.WorkoutRoutineId });
                    table.ForeignKey(
                        name: "FK_UserWorkoutRoutines_AspNetUsers_AssignedByTrainerId",
                        column: x => x.AssignedByTrainerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWorkoutRoutines_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWorkoutRoutines_WorkoutRoutines_WorkoutRoutineId",
                        column: x => x.WorkoutRoutineId,
                        principalTable: "WorkoutRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutines_TrainerId",
                table: "WorkoutRoutines",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlans_ApplicationUserId",
                table: "NutritionPlans",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkoutRoutines_AssignedByTrainerId",
                table: "UserWorkoutRoutines",
                column: "AssignedByTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkoutRoutines_WorkoutRoutineId",
                table: "UserWorkoutRoutines",
                column: "WorkoutRoutineId");

            migrationBuilder.AddForeignKey(
                name: "FK_NutritionPlans_AspNetUsers_ApplicationUserId",
                table: "NutritionPlans",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutRoutines_AspNetUsers_TrainerId",
                table: "WorkoutRoutines",
                column: "TrainerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NutritionPlans_AspNetUsers_ApplicationUserId",
                table: "NutritionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutRoutines_AspNetUsers_TrainerId",
                table: "WorkoutRoutines");

            migrationBuilder.DropTable(
                name: "UserWorkoutRoutines");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutRoutines_TrainerId",
                table: "WorkoutRoutines");

            migrationBuilder.DropIndex(
                name: "IX_NutritionPlans_ApplicationUserId",
                table: "NutritionPlans");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "NutritionPlans");
        }
    }
}
