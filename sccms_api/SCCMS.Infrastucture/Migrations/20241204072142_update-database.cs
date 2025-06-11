using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCCMS.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class updatedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isCancel",
                table: "StaffFreeTimes",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isCancel",
                table: "StaffFreeTimes");
        }
    }
}
