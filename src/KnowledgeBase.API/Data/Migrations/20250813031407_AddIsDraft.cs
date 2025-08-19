using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeBase.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Notes");
        }
    }
}
