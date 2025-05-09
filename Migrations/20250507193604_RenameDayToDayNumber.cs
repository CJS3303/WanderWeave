using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1.Migrations
{
    /// <inheritdoc />
    public partial class RenameDayToDayNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Trips_TripId",
                table: "Days");

            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Days_dayId",
                table: "Stops");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "dayId",
                table: "Stops",
                newName: "DayId");

            migrationBuilder.RenameIndex(
                name: "IX_Stops_dayId",
                table: "Stops",
                newName: "IX_Stops_DayId");

            migrationBuilder.RenameColumn(
                name: "day",
                table: "Days",
                newName: "DayNumber");

            migrationBuilder.UpdateData(
                table: "Trips",
                keyColumn: "UserId",
                keyValue: null,
                column: "UserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Trips",
                type: "varchar(95)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(95)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Days",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Trips_TripId",
                table: "Days",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Days_DayId",
                table: "Stops",
                column: "DayId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Trips_TripId",
                table: "Days");

            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Days_DayId",
                table: "Stops");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "DayId",
                table: "Stops",
                newName: "dayId");

            migrationBuilder.RenameIndex(
                name: "IX_Stops_DayId",
                table: "Stops",
                newName: "IX_Stops_dayId");

            migrationBuilder.RenameColumn(
                name: "DayNumber",
                table: "Days",
                newName: "day");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Trips",
                type: "varchar(95)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(95)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Days",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Trips_TripId",
                table: "Days",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Days_dayId",
                table: "Stops",
                column: "dayId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
