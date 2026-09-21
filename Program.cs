using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Ρύθμιση SQLite
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlite("Data Source=todos.db"));

// Ρύθμιση για Cloud PostgreSQL (διαβάζει το Connection String από το Render)
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
//    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(connectionString)); // <-- Αλλαγή σε UseNpgsql

// Δυναμική κατασκευή του Connection String από τις καθαρές μεταβλητές του Render
var envHost = Environment.GetEnvironmentVariable("PGHOST");
var envPort = Environment.GetEnvironmentVariable("PGPORT");
var envUser = Environment.GetEnvironmentVariable("PGUSER");
var envPass = Environment.GetEnvironmentVariable("PGPASSWORD");
var envDb   = Environment.GetEnvironmentVariable("PGDATABASE");

string connectionString;

if (!string.IsNullOrEmpty(envHost))
{
    // Αν είμαστε στο Render, φτιάξε το string από τις αυτόνομες μεταβλητές
    connectionString = $"Server={envHost};Port={envPort};User Id={envUser};Password={envPass};Database={envDb};Ssl Mode=Require;Trust Server Certificate=true;";
}
else
{
    // Αν είμαστε τοπικά, διάβασε το appsettings (ή βάλε ένα default fallback)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// 2. Ρύθμιση CORS για όλους τους browsers (Chrome & Brave)
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

// Αυτόματη δημιουργία της βάσης δεδομένων
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowReact");

// ==========================================
// 3. MINIMAL API ENDPOINTS (Αντικαθιστά τον Controller)
// ==========================================

// GET: api/todos?username=Ntina
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
