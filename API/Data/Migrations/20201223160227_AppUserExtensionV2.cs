using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Data.Migrations
{
    public partial class AppUserExtensionV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lookingFor",
                table: "Users",
                newName: "LookingFor");

            migrationBuilder.RenameColumn(
                name: "KnowsAs",
                table: "Users",
                newName: "KnownAs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LookingFor",
                table: "Users",
                newName: "lookingFor");

            migrationBuilder.RenameColumn(
                name: "KnownAs",
                table: "Users",
                newName: "KnowsAs");
        }
    }
}
