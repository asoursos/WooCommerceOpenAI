using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class function_match_posts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if(migrationBuilder.IsNpgsql())
            {
                migrationBuilder.Sql(@"
CREATE function match_posts (
  query_embedding vector(1536),
  match_threshold float,
  match_count int
)
returns table (
  id bigint,
  name text,
  name_similarity FLOAT,
  description_similarity FLOAT
)
language sql STABLE

AS '
  select
    wcp.id,
    wcp.name,
    1 - (wcp.name_vector <=> query_embedding) as name_similarity,
    1 - (wcp.description_vector <=> query_embedding) as description_similarity
  from woocommerce_posts wcp
  where (1 - (wcp.name_vector <=> query_embedding) > match_threshold) or (1 - (wcp.description_vector <=> query_embedding) > match_threshold)
  order by name_similarity desc
  limit match_count;
';
");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP FUNCTION match_posts(vector(1536), FLOAT, INT)");
        }
    }
}
