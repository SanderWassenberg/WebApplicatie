using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwapGame_API.Migrations
{
    /// <inheritdoc />
    public partial class added_User_HashedPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashedPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "HashedPassword",
                value: "AQAAAAIAAYagAAAAEBZCtMd/03Zx7M+OUNNZqiGkTqH2SySNZYh/eLLDKov5u0p3ewSPvVsGaO4f6vCD/w==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashedPassword",
                table: "Users");
        }
    }
}
