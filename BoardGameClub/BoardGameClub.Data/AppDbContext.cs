using Microsoft.EntityFrameworkCore;

namespace BoardGameClub.Data;

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Rental> Rentals => Set<Rental>();

    public static string DbPath
    {
        get
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "boardgameclub.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}