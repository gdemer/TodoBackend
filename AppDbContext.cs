using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Αναγκάζουμε την PostgreSQL να χαρτογραφήσει σωστά τον πίνακα "Todos"
        modelBuilder.Entity<Todo>().ToTable("Todos");
    }
}
