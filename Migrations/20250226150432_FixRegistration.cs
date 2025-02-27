using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementServer.Migrations
{
    /// <inheritdoc />
    public partial class FixRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventID",
                table: "Registrations");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Registrations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventID",
                table: "Registrations",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "EventID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventID",
                table: "Registrations");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Registrations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventID",
                table: "Registrations",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "EventID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
