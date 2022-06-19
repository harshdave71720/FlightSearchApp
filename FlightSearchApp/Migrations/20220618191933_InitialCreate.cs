using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FlightSearchApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Info_Origin = table.Column<string>(nullable: false),
                    Info_DepartureTime = table.Column<DateTime>(nullable: false),
                    Info_Destination = table.Column<string>(nullable: false),
                    Info_ArrivalTime = table.Column<DateTime>(nullable: false),
                    Info_Price = table.Column<double>(nullable: false),
                    Info_Provider = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
