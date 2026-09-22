using Microsoft.EntityFrameworkCore.Migrations;
namespace Project1.Migrations;
public partial class AddStopTimes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<TimeSpan>(name: "ArrivalTime", table: "Stops", type: "time(6)", nullable: true);
        migrationBuilder.AddColumn<TimeSpan>(name: "DepartureTime", table: "Stops", type: "time(6)", nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ArrivalTime", table: "Stops");
        migrationBuilder.DropColumn(name: "DepartureTime", table: "Stops");
    }
}
