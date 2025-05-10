using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriPlat.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProfessionalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyNutritionistId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MyTrainerId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MyNutritionistId",
                table: "AspNetUsers",
                column: "MyNutritionistId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MyTrainerId",
                table: "AspNetUsers",
                column: "MyTrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_MyNutritionistId",
                table: "AspNetUsers",
                column: "MyNutritionistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_MyTrainerId",
                table: "AspNetUsers",
                column: "MyTrainerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_MyNutritionistId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_MyTrainerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MyNutritionistId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MyTrainerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MyNutritionistId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MyTrainerId",
                table: "AspNetUsers");
        }
    }
}
