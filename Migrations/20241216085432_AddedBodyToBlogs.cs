using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTechBlogsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedBodyToBlogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "blogs");
        }
    }
}
