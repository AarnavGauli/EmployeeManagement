using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomIdentity.Migrations
{
    /// <inheritdoc />
    public partial class leavetypeadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "LeaveDetails",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "LeaveType",
                table: "LeaveDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "LeaveDetails",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "LeaveDetails");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "LeaveDetails");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "LeaveDetails");
        }
    }
}
