using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "woocommerce_posts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    name_embedding_hash_id = table.Column<string>(type: "text", nullable: true),
                    name_embedding_vector = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    description_embedding_hash_id = table.Column<string>(type: "text", nullable: true),
                    description_embedding_vector = table.Column<Vector>(type: "vector(1536)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_woocommerce_posts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_woocommerce_posts_description_embedding_vector",
                table: "woocommerce_posts",
                column: "description_embedding_vector")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_woocommerce_posts_name_embedding_vector",
                table: "woocommerce_posts",
                column: "name_embedding_vector")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "woocommerce_posts");
        }
    }
}
