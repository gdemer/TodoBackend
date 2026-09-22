using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using EFCore.NamingConventions; // <-- ΑΥΤΗ Η ΓΡΑΜΜΗ ΛΕΙΠΕΙ ΚΑΙ ΠΡΕΠΕΙ ΝΑ ΠΡΟΣΤΕΘΕΙ!

var builder = WebApplication.CreateBuilder(args);

// 1. Ενεργοποίηση Controllers / Minimal API υποδομής
builder.Services.AddControllers();

// 2. Δυναμική κατασκευή του Connection String (PostgreSQL)
var envHost = Environment.GetEnvironmentVariable("PGHOST");
var envPort = Environment.GetEnvironmentVariable("PGPORT");
var envUser = Environment.GetEnvironmentVariable("PGUSER");
var envPass = Environment.GetEnvironmentVariable("PGPASSWORD");
var envDb   = Environment.GetEnvironmentVariable("PGDATABASE");

string connectionString;

if (!string.IsNullOrEmpty(envHost))
{
    connectionString = $"Server={envHost};Port={envPort};User Id={envUser};Password={envPass};Database={envDb};Ssl Mode=Require;Trust Server Certificate=true;";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}

// Σύνδεση με PostgreSQL και αυτόματη μετατροπή ονομάτων σε snake_case
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConventions()); // <-- ΔΙΟΡΘΩΣΗ: Τώρα θα αναγνωριστεί κανονικά

// 3. Ενεργοποίηση CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowReact");

// ==========================================
// 4. MINIMAL API ENDPOINTS
// ==========================================

// GET: api/todos?username=George
app.MapGet("/api/todos", async (string username, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(username)) return Results.BadRequest("Το username είναι υποχρεωτικό.");
    var userTodos = await db.Todos.Where(t => t.Username == username).ToListAsync();
    return Results.Ok(userTodos);
});

// POST: api/todos
app.MapPost("/api/todos", async (Todo todo, AppDbContext db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

// PUT: api/todos/5
app.MapPut("/api/todos/{id}", async (int id, Todo updatedTodo, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null) return Results.NotFound();

    todo.Text = updatedTodo.Text;
    todo.Completed = updatedTodo.Completed;
    todo.DueDate = updatedTodo.DueDate;
    todo.Priority = updatedTodo.Priority;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE: api/todos/5
app.MapDelete("/api/todos/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
