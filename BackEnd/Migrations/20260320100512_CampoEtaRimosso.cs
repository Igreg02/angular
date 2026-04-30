using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RubricaSemplice.Api.Migrations
{
    /// <inheritdoc />
    public partial class CampoEtaRimosso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eta",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Eta",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
