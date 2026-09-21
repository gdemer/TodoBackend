using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Προσθήκη των Controllers
builder.Services.AddControllers();

// 2. Δυναμική κατασκευή του Connection String από τις 5 αυτόνομες μεταβλητές του Render (PostgreSQL)
var envHost = Environment.GetEnvironmentVariable("PGHOST");
var envPort = Environment.GetEnvironmentVariable("PGPORT");
var envUser = Environment.GetEnvironmentVariable("PGUSER");
var envPass = Environment.GetEnvironmentVariable("PGPASSWORD");
var envDb   = Environment.GetEnvironmentVariable("PGDATABASE");

string connectionString;

if (!string.IsNullOrEmpty(envHost))
{
    // Αν είμαστε στο Render, κατασκευάζεται το string για την PostgreSQL
    connectionString = $"Server={envHost};Port={envPort};User Id={envUser};Password={envPass};Database={envDb};Ssl Mode=Require;Trust Server Certificate=true;";
}
else
{
    // Αν είμαστε τοπικά (Development fallback)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Ενεργοποίηση CORS για σύνδεση με τη React
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

// 4. Ενεργοποίηση των απαραίτητων Middlewares
app.UseCors("AllowReact");
app.UseAuthorization();

// 5. Χαρτογράφηση των Endpoints
app.MapControllers();

app.Run();
