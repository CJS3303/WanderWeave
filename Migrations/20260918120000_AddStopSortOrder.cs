using Microsoft.EntityFrameworkCore.Migrations;

namespace Project1.Migrations;

public partial class AddStopSortOrder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "SortOrder", table: "Stops",
            type: "int", nullable: false, defaultValue: 0);
        // Preserve the existing insertion order for old stops.
        migrationBuilder.Sql("UPDATE `Stops` SET `SortOrder` = `Id`;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "SortOrder", table: "Stops");
    }
}
