using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBookyWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeads",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: null); // Specify your default value here
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeads");
        }
    }
}
