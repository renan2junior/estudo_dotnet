using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapPost("/categorias", async(Categoria categoria, AppDbContext db)=>
{
  db.Categorias.Add(categoria);
  await db.SaveChangesAsync();
  return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
});

app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.ToListAsync() );

app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) => {
    return await db.Categorias.FindAsync(id) is Categoria categoria ? Results.Ok(categoria): Results.NotFound();
  }
);

app.MapPut("/categorias/{id:int}", async(int id, Categoria categoria, AppDbContext db) => {

    if(categoria.CategoriaId != id){
      return Results.BadRequest();
    }

    var categoriaFind = await db.Categorias.FindAsync(id);

    if(categoriaFind is null) return Results.NotFound();

    categoriaFind.Nome = categoria.Nome;
    categoriaFind.Descricao = categoria.Descricao;

    // categoriaFind = categoria;

    await db.SaveChangesAsync();
    return Results.Ok(categoriaFind);


});

app.MapDelete("/categorias/{id:int}",async (int id, AppDbContext db)=>{

  var categoriaFind = await db.Categorias.FindAsync(id);

  if(categoriaFind is null) return Results.NotFound();

  db.Remove(categoriaFind);
  await db.SaveChangesAsync();

  return Results.NoContent();

});



app.Run();
