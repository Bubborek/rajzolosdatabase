using Microsoft.EntityFrameworkCore;
using System.Drawing;

public class DrawingDbContext : DbContext
{
    public DbSet<Drawing> Drawings { get; set; }
    public DbSet<Point> Points { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=DrawingDB;Trusted_Connection=True;");
    }
}
