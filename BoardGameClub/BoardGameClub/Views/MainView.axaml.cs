using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BoardGameClub.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGameClub.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // В режиме предпросмотра в редакторе базу не открываем
        if (Design.IsDesignMode)
            return;

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }

        LoadAll();
    }

    // Загрузка всех списков

    private void LoadAll()
    {
        using (var db = new AppDbContext())
        {
            var clients = db.Clients.OrderBy(c => c.FullName).ToList();
            ClientsList.ItemsSource = clients;
            ClientCombo.ItemsSource = clients;
        }

        using (var db = new AppDbContext())
        {
            var games = db.Games.OrderBy(g => g.Title).ToList();
            GamesList.ItemsSource = games;
            GameCombo.ItemsSource = games;
        }

        using (var db = new AppDbContext())
        {
            RentalsList.ItemsSource = db.Rentals
                .Include(r => r.Client)
                .Include(r => r.Game)
                .OrderByDescending(r => r.StartDate)
                .ToList();
        }

        using (var db = new AppDbContext())
        {
            ActiveRentalsList.ItemsSource = db.Rentals
                .Include(r => r.Client)
                .Include(r => r.Game)
                .Where(r => r.ReturnDate == null)
                .ToList();
        }
    }

    // Сообщения

    private static void ShowError(TextBlock box, string text)
    {
        box.Foreground = Brushes.Crimson;
        box.Text = text;
    }

    private static void ShowErrors(TextBlock box, List<string> errors)
        => ShowError(box, "Ошибка: " + string.Join("; ", errors) + ".");

    private static void ShowOk(TextBlock box, string text)
    {
        box.Foreground = Brushes.SeaGreen;
        box.Text = text;
    }

    private static string ErrorText(Exception ex)
        => "Не удалось выполнить действие: " + (ex.InnerException?.Message ?? ex.Message);

    // 
    //                       КЛИЕНТЫ
    //

    // Читает и проверяет форму. Если есть ошибки, показывает их и возвращает null.
    private Client? ReadClientForm()
    {
        var errors = new List<string>();

        var name = ClientNameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
            errors.Add("не заполнено имя");

        var date = ClientDatePicker.SelectedDate;
        if (date == null)
            errors.Add("не выбрана дата регистрации");

        if (errors.Count > 0)
        {
            ShowErrors(ClientsMessage, errors);
            return null;
        }

        var phone = ClientPhoneBox.Text?.Trim();
        return new Client
        {
            FullName = name!,
            Phone = string.IsNullOrEmpty(phone) ? null : phone,
            RegistrationDate = date!.Value.Date,
            HasClubCard = ClientCardCheck.IsChecked == true
        };
    }

    private void ClearClientForm()
    {
        ClientNameBox.Text = "";
        ClientPhoneBox.Text = "";
        ClientDatePicker.SelectedDate = null;
        ClientCardCheck.IsChecked = false;
        ClientsList.SelectedItem = null;
    }

    private void ClientsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // После обновления списка выбор сбрасывается, это не ошибка
        if (ClientsList.SelectedItem is not Client c)
            return;

        ClientNameBox.Text = c.FullName;
        ClientPhoneBox.Text = c.Phone;
        ClientDatePicker.SelectedDate = c.RegistrationDate;
        ClientCardCheck.IsChecked = c.HasClubCard;
    }

    private void AddClient_Click(object? sender, RoutedEventArgs e)
    {
        var data = ReadClientForm();
        if (data == null)
            return;

        try
        {
            using var db = new AppDbContext();
            db.Clients.Add(data);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(ClientsMessage, ErrorText(ex));
            return;
        }

        ClearClientForm();
        LoadAll();
        ShowOk(ClientsMessage, $"Клиент «{data.FullName}» добавлен.");
    }

    private void SaveClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsList.SelectedItem is not Client selected)
        {
            ShowError(ClientsMessage, "Сначала выберите клиента в списке.");
            return;
        }

        var data = ReadClientForm();
        if (data == null)
            return;

        try
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(selected.Id); // берём запись из базы заново
            if (client == null)
            {
                ShowError(ClientsMessage, "Запись не найдена: возможно, она уже удалена.");
                LoadAll();
                return;
            }

            client.FullName = data.FullName;
            client.Phone = data.Phone;
            client.RegistrationDate = data.RegistrationDate;
            client.HasClubCard = data.HasClubCard;
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(ClientsMessage, ErrorText(ex));
            return;
        }

        ClearClientForm();
        LoadAll();
        ShowOk(ClientsMessage, "Изменения сохранены.");
    }

    private void DeleteClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsList.SelectedItem is not Client selected)
        {
            ShowError(ClientsMessage, "Сначала выберите клиента в списке.");
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(selected.Id);
            if (client == null)
            {
                ShowError(ClientsMessage, "Запись не найдена: возможно, она уже удалена.");
                LoadAll();
                return;
            }

            db.Clients.Remove(client);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(ClientsMessage, ErrorText(ex));
            return;
        }

        ClearClientForm();
        LoadAll();
        ShowOk(ClientsMessage, "Клиент удалён.");
    }

    // 
    //                         ИГРЫ
    // 

    private Game? ReadGameForm()
    {
        var errors = new List<string>();

        var title = GameTitleBox.Text?.Trim();
        if (string.IsNullOrEmpty(title))
            errors.Add("не заполнено название");

        int min, max, complexity, price;

        if (!int.TryParse(GameMinBox.Text?.Trim(), out min) || min < 1)
            errors.Add("«Минимум игроков» должен быть целым числом от 1");

        if (!int.TryParse(GameMaxBox.Text?.Trim(), out max) || max < 1)
            errors.Add("«Максимум игроков» должен быть целым числом от 1");
        else if (min >= 1 && max < min)
            errors.Add("максимум игроков не может быть меньше минимума");

        if (!int.TryParse(GameComplexityBox.Text?.Trim(), out complexity) || complexity < 1 || complexity > 5)
            errors.Add("«Сложность» должна быть целым числом от 1 до 5");

        if (!int.TryParse(GamePriceBox.Text?.Trim(), out price) || price < 0)
            errors.Add("«Цена за сутки» должна быть целым неотрицательным числом");

        if (errors.Count > 0)
        {
            ShowErrors(GamesMessage, errors);
            return null;
        }

        var description = GameDescriptionBox.Text?.Trim();
        return new Game
        {
            Title = title!,
            MinPlayers = min,
            MaxPlayers = max,
            Complexity = complexity,
            PricePerDay = price,
            Description = string.IsNullOrEmpty(description) ? null : description
        };
    }

    private void ClearGameForm()
    {
        GameTitleBox.Text = "";
        GameMinBox.Text = "";
        GameMaxBox.Text = "";
        GameComplexityBox.Text = "";
        GamePriceBox.Text = "";
        GameDescriptionBox.Text = "";
        GamesList.SelectedItem = null;
    }

    private void GamesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game g)
            return;

        GameTitleBox.Text = g.Title;
        GameMinBox.Text = g.MinPlayers.ToString();
        GameMaxBox.Text = g.MaxPlayers.ToString();
        GameComplexityBox.Text = g.Complexity.ToString();
        GamePriceBox.Text = g.PricePerDay.ToString();
        GameDescriptionBox.Text = g.Description;
    }

    private void AddGame_Click(object? sender, RoutedEventArgs e)
    {
        var data = ReadGameForm();
        if (data == null)
            return;

        try
        {
            using var db = new AppDbContext();
            db.Games.Add(data);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(GamesMessage, ErrorText(ex));
            return;
        }

        ClearGameForm();
        LoadAll();
        ShowOk(GamesMessage, $"Игра «{data.Title}» добавлена.");
    }

    private void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game selected)
        {
            ShowError(GamesMessage, "Сначала выберите игру в списке.");
            return;
        }

        var data = ReadGameForm();
        if (data == null)
            return;

        try
        {
            using var db = new AppDbContext();
            var game = db.Games.Find(selected.Id);
            if (game == null)
            {
                ShowError(GamesMessage, "Запись не найдена: возможно, она уже удалена.");
                LoadAll();
                return;
            }

            game.Title = data.Title;
            game.MinPlayers = data.MinPlayers;
            game.MaxPlayers = data.MaxPlayers;
            game.Complexity = data.Complexity;
            game.PricePerDay = data.PricePerDay;
            game.Description = data.Description;
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(GamesMessage, ErrorText(ex));
            return;
        }

        ClearGameForm();
        LoadAll();
        ShowOk(GamesMessage, "Изменения сохранены.");
    }

    private void DeleteGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game selected)
        {
            ShowError(GamesMessage, "Сначала выберите игру в списке.");
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var game = db.Games.Find(selected.Id);
            if (game == null)
            {
                ShowError(GamesMessage, "Запись не найдена: возможно, она уже удалена.");
                LoadAll();
                return;
            }

            db.Games.Remove(game);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(GamesMessage, ErrorText(ex));
            return;
        }

        ClearGameForm();
        LoadAll();
        ShowOk(GamesMessage, "Игра удалена.");
    }

    // 
    //                        АРЕНДЫ
    // 

    private void AddRental_Click(object? sender, RoutedEventArgs e)
    {
        var errors = new List<string>();

        var client = ClientCombo.SelectedItem as Client;
        if (client == null)
            errors.Add("не выбран клиент");

        var game = GameCombo.SelectedItem as Game;
        if (game == null)
            errors.Add("не выбрана игра");

        var start = RentalStartPicker.SelectedDate;
        if (start == null)
            errors.Add("не выбрана дата выдачи");

        var returnDate = RentalReturnPicker.SelectedDate; // необязательная
        if (start != null && returnDate != null && returnDate < start)
            errors.Add("дата возврата не может быть раньше даты выдачи");

        if (!int.TryParse(RentalPriceBox.Text?.Trim(), out var price) || price < 0)
            errors.Add("«Итоговая стоимость» должна быть целым неотрицательным числом");

        if (errors.Count > 0)
        {
            ShowErrors(RentalsMessage, errors);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = new Rental
            {
                ClientId = client!.Id,   // записываем Id, а не сам объект
                GameId = game!.Id,
                StartDate = start!.Value.Date,
                ReturnDate = returnDate?.Date,
                TotalPrice = price
            };
            db.Rentals.Add(rental);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(RentalsMessage, ErrorText(ex));
            return;
        }

        RentalPriceBox.Text = "";
        RentalStartPicker.SelectedDate = null;
        RentalReturnPicker.SelectedDate = null;
        LoadAll();
        ShowOk(RentalsMessage, "Аренда добавлена.");
    }

    private void DeleteRental_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsList.SelectedItem is not Rental selected)
        {
            ShowError(RentalsMessage, "Сначала выберите аренду в списке.");
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(selected.Id);
            if (rental == null)
            {
                ShowError(RentalsMessage, "Запись не найдена: возможно, она уже удалена.");
                LoadAll();
                return;
            }

            db.Rentals.Remove(rental);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            ShowError(RentalsMessage, ErrorText(ex));
            return;
        }

        LoadAll();
        ShowOk(RentalsMessage, "Аренда удалена.");
    }
}