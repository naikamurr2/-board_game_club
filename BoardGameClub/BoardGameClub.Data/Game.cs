namespace BoardGameClub.Data;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int Complexity { get; set; }
    public int PricePerDay { get; set; }
    public string? Description { get; set; }

    public List<Rental> Rentals { get; set; } = new();

    public override string ToString()
        => $"{Title} ({MinPlayers}-{MaxPlayers} игроков, сложность {Complexity}, {PricePerDay} руб./сутки)";
}