using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class desc_vector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "description_vector",
                table: "woocommerce_posts",
                type: "vector(1536)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description_vector",
                table: "woocommerce_posts");
        }
    }
}
