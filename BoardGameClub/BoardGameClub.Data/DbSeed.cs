namespace BoardGameClub.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        // Если в базе уже есть записи, ничего не добавляем
        if (db.Clients.Any() || db.Games.Any() || db.Rentals.Any())
            return;

        var clients = new List<Client>
        {
            new() { FullName = "Иван Петров", Phone = "+7 900 111-22-33",
                    RegistrationDate = new DateTime(2026, 9, 5), HasClubCard = true },
            new() { FullName = "Мария Сидорова", Phone = "+7 900 222-33-44",
                    RegistrationDate = new DateTime(2026, 10, 12), HasClubCard = false },
            new() { FullName = "Алексей Смирнов", Phone = null,
                    RegistrationDate = new DateTime(2026, 10, 20), HasClubCard = true },
            new() { FullName = "Ольга Кузнецова", Phone = "+7 900 444-55-66",
                    RegistrationDate = new DateTime(2026, 11, 2), HasClubCard = false },
        };

        var games = new List<Game>
        {
            new() { Title = "Каркассон", MinPlayers = 2, MaxPlayers = 5, Complexity = 2,
                    PricePerDay = 150, Description = "Строим города и дороги из плиток" },
            new() { Title = "Катан", MinPlayers = 3, MaxPlayers = 4, Complexity = 3,
                    PricePerDay = 200, Description = "Торговля и строительство на острове" },
            new() { Title = "Мафия", MinPlayers = 6, MaxPlayers = 12, Complexity = 1,
                    PricePerDay = 100, Description = null },
            new() { Title = "Манчкин", MinPlayers = 3, MaxPlayers = 6, Complexity = 2,
                    PricePerDay = 120, Description = "Карточная игра про подземелья с юмором" },
            new() { Title = "Gloomhaven", MinPlayers = 1, MaxPlayers = 4, Complexity = 5,
                    PricePerDay = 400, Description = "Большая приключенческая кампания" },
        };

        var rentals = new List<Rental>
        {
            new() { Client = clients[0], Game = games[0],
                    StartDate = new DateTime(2026, 11, 1, 18, 0, 0),
                    ReturnDate = new DateTime(2026, 11, 3, 12, 30, 0), TotalPrice = 270 },
            new() { Client = clients[0], Game = games[1],
                    StartDate = new DateTime(2026, 11, 8, 17, 15, 0),
                    ReturnDate = new DateTime(2026, 11, 9, 19, 0, 0), TotalPrice = 180 },
            new() { Client = clients[1], Game = games[2],
                    StartDate = new DateTime(2026, 11, 5, 19, 0, 0),
                    ReturnDate = new DateTime(2026, 11, 6, 21, 0, 0), TotalPrice = 100 },
            new() { Client = clients[2], Game = games[4],
                    StartDate = new DateTime(2026, 11, 10, 16, 0, 0),
                    ReturnDate = null, TotalPrice = 360 },
            new() { Client = clients[3], Game = games[3],
                    StartDate = new DateTime(2026, 11, 7, 14, 0, 0),
                    ReturnDate = new DateTime(2026, 11, 9, 14, 0, 0), TotalPrice = 240 },
        };

        db.Clients.AddRange(clients);
        db.Games.AddRange(games);
        db.Rentals.AddRange(rentals);
        db.SaveChanges();
    }
}