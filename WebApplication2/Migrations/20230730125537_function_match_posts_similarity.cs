using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class function_match_posts_similarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsNpgsql())
            {
                migrationBuilder.Sql(@"
CREATE function match_posts_similarity (
  query_term text,
  match_count int
)
returns table (
  id bigint,
  name text,
  name_similarity FLOAT
)
language sql STABLE

AS '
SELECT id, name, SIMILARITY(metaphone(name,10), metaphone(query_term,10)) AS name_similarity
FROM woocommerce_posts
ORDER BY SIMILARITY(metaphone(name,10), metaphone(query_term,10)) DESC
LIMIT match_count;
';
");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
