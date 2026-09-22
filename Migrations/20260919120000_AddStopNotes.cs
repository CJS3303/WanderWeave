using Microsoft.EntityFrameworkCore.Migrations;
namespace Project1.Migrations;
public partial class AddStopNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Notes", table: "Stops", type: "text", maxLength: 4000, nullable: true);
        migrationBuilder.Sql("UPDATE `Stops` SET `Notes` = '' WHERE `Notes` IS NULL;");
        migrationBuilder.AlterColumn<string>(name: "Notes", table: "Stops", type: "text", maxLength: 4000, nullable: false,
            oldClrType: typeof(string), oldType: "text", oldMaxLength: 4000, oldNullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "Notes", table: "Stops");
}
