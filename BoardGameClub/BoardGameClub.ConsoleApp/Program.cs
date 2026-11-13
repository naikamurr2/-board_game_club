using System.Text;
using BoardGameClub.Data;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"Файл базы: {AppDbContext.DbPath}");
Console.WriteLine();

using var db = new AppDbContext();
db.Database.EnsureCreated();
DbSeed.Seed(db);

var clients = db.Clients.ToList();
Console.WriteLine($"=== Clients (записей: {clients.Count}) ===");
foreach (var client in clients)
    Console.WriteLine(client);
Console.WriteLine();

var games = db.Games.ToList();
Console.WriteLine($"=== Games (записей: {games.Count}) ===");
foreach (var game in games)
    Console.WriteLine(game);
Console.WriteLine();

var rentals = db.Rentals
    .Include(r => r.Client)
    .Include(r => r.Game)
    .ToList();
Console.WriteLine($"=== Rentals (записей: {rentals.Count}) ===");
foreach (var rental in rentals)
    Console.WriteLine(rental);