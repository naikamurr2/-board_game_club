using Avalonia.Controls;
using BoardGameClub.Data;
using Microsoft.EntityFrameworkCore;
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

        // 1. Создаём базу и заполняем её тестовыми данными
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }

        // 2. Загружаем списки: на каждое действие свой контекст
        using (var db = new AppDbContext())
        {
            ClientsList.ItemsSource = db.Clients.ToList();
        }

        using (var db = new AppDbContext())
        {
            GamesList.ItemsSource = db.Games.ToList();
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
}