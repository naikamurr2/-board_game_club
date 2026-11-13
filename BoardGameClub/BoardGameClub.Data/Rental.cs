namespace BoardGameClub.Data;

public class Rental
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int TotalPrice { get; set; }

    public override string ToString()
    {
        var returned = ReturnDate.HasValue
            ? ReturnDate.Value.ToString("dd.MM.yyyy")
            : "ещё не возвращена";

        return $"{Client?.FullName} взял(а) «{Game?.Title}»: " +
               $"{StartDate:dd.MM.yyyy} → {returned}, {TotalPrice} руб.";
    }
}