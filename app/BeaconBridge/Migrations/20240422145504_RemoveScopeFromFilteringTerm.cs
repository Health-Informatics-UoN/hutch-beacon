using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeaconBridge.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScopeFromFilteringTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scope",
                table: "FilteringTerms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "FilteringTerms",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
