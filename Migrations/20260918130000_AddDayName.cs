using Microsoft.EntityFrameworkCore.Migrations;

namespace Project1.Migrations;

public partial class AddDayName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Name", table: "Days",
            type: "varchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.Sql("UPDATE `Days` SET `Name` = CONCAT('Untitled ', `DayNumber` + 1) WHERE `Name` IS NULL OR TRIM(`Name`) = '';");
        migrationBuilder.AlterColumn<string>(name: "Name", table: "Days",
            type: "varchar(100)", maxLength: 100, nullable: false,
            oldClrType: typeof(string), oldType: "varchar(100)", oldMaxLength: 100, oldNullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Name", table: "Days");
    }
}
