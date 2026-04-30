using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RubricaSemplice.Api.Migrations
{
    /// <inheritdoc />
    public partial class CampoEtaUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Eta",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eta",
                table: "AspNetUsers");
        }
    }
}
