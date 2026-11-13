namespace BoardGameClub.Data;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool HasClubCard { get; set; }

    public List<Rental> Rentals { get; set; } = new();

    public override string ToString()
        => $"{FullName}, тел.: {Phone ?? "не указан"}";
}