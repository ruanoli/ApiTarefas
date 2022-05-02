using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options
    => options.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tarefas", async (AppDbContext context) 
    => await context.Tarefas.ToListAsync());


app.MapGet("/tarefas{id:int}", async (int id, AppDbContext context) 
    => await context.Tarefas.FindAsync(id) 
    is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound()
);

app.MapGet("/tarefas/concluidas", async (AppDbContext context) 
    => await context.Tarefas.Where(x => x.Done).ToListAsync()
 );

app.MapPost("/tarefas", async (AppDbContext context, Tarefa tarefa) => {
    await context.Tarefas.AddAsync(tarefa);
    await context.SaveChangesAsync();

    return Results.Created($"/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id:int}", async (int id, AppDbContext context, Tarefa inputTarefa) => {
    var tarefa = await context.Tarefas.FindAsync(id);
    if(tarefa is null)
        return Results.NotFound();

    tarefa.Name = inputTarefa.Name;
    tarefa.Done = inputTarefa.Done;

    context.Tarefas.Update(tarefa);
    await context.SaveChangesAsync();

    return Results.Ok(tarefa);
});

app.MapDelete("/tarefas/{id:int}", async (int id, AppDbContext context) => {
    if(await context.Tarefas.FindAsync(id) is Tarefa tarefa) {
        context.Tarefas.Remove(tarefa);
        await context.SaveChangesAsync();

        return Results.Ok(tarefa);
    }
    return Results.NotFound();
  

    
});
app.Run();

class Tarefa {
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool Done { get; set; }
}

class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) :base(options) {

    }
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

}