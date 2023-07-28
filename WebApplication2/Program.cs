using Microsoft.EntityFrameworkCore;
using Pgvector;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection(nameof(OpenAISettings)));
builder.Services.Configure<WooCommerceSettings>(
    builder.Configuration.GetSection(nameof(WooCommerceSettings)));

builder.Services.AddDbContext<ItemDbContext>();
builder.Services.AddTransient<IEmbeddingsService, EmbeddingsService>();
builder.Services.AddTransient<IWooCommerceService, WooCommerceService>();
builder.Services.AddTransient<ITokensService, TokensService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
    //ctx.Database.EnsureDeleted();
    //ctx.Database.EnsureCreated();
    ctx.Database.Migrate();

    //ctx.Posts.Add(new Post { NameVector = new Vector(new float[] { 1, 1, 1 }) });
    //ctx.Posts.Add(new Post { NameVector = new Vector(new float[] { 2, 2, 2 }) });
    //ctx.Posts.Add(new Post { NameVector = new Vector(new float[] { 1, 1, 2 }) });
    //ctx.SaveChanges();

    //var embedding = new Vector(new float[] { 1, 1, 1 });
    //var items = await ctx.Posts.FromSql($"SELECT * FROM woocommerce_posts ORDER BY embedding <-> {embedding} LIMIT 5").ToListAsync();
    //foreach (Post item in items)
    //{
    //    if (item.NameVector != null)
    //    {
    //        Console.WriteLine(item.NameVector);
    //    }
    //}
}


app.Run();
