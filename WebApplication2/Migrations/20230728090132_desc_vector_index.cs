using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class desc_vector_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_woocommerce_posts_description_vector",
                table: "woocommerce_posts",
                column: "description_vector")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_woocommerce_posts_description_vector",
                table: "woocommerce_posts");
        }
    }
}
