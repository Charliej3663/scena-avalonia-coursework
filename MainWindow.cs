using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.EntityFrameworkCore;
using ScenaAvalonia.Data;
using ScenaAvalonia.Models;
using System.Text.Json.Serialization;

namespace ScenaAvalonia;

public sealed class MainWindow : Window
{
    private static readonly string[] Categories = ["Актёрское мастерство", "Сценическая речь", "Пластика и движение"];
    private static readonly Course[] Courses =
    [
        new(0,"Актёрское мастерство: первая сцена","Внимание, партнёрство и сценическое действие: от первого упражнения до небольшого этюда.",0,15000,"Сергеева Анна Викторовна","course-0.jpg",["Пн, Ср · 18:30–20:00","Сб, Вс · 11:00–12:30"]),
        new(1,"Сценическая речь и дикция","Дыхание, артикуляция и работа с текстом для ясной и свободной речи.",1,12000,"Волков Илья Андреевич","course-1.jpg",["Вт, Чт · 19:00–20:30","Сб · 10:00–11:30"]),
        new(2,"Пластика актёра","Координация, осанка и выразительное движение в безопасном темпе.",2,13500,"Белова Мария Олеговна","course-2.jpg",["Пн, Ср, Пт · 17:30–19:00","Вс · 12:00–13:30"]),
        new(3,"Импровизация для начинающих","Воображение, спонтанность и взаимодействие с партнёром без готового текста.",0,11000,"Громов Дмитрий Николаевич","course-3.jpg",["Вт, Чт · 20:00–21:30","Сб, Вс · 14:00–15:30"]),
        new(4,"Голос и публичное выступление","Как выстроить выступление, управлять голосом и удерживать внимание аудитории.",1,12500,"Тихонова Ольга Сергеевна","course-4.jpg",["Пн, Ср · 19:30–21:00","Сб · 12:00–13:30"]),
        new(5,"Театр и работа с эмоциями","Исследование эмоций персонажа и выразительности через игровые этюды.",0,14000,"Миронова Елена Павловна","course-5.jpg",["Вт, Пт · 18:00–19:30","Вс · 15:00–16:30"]),
        new(6,"Сценическое движение","Пространство сцены, ритм и создание выразительного физического образа.",2,13000,"Лебедев Артём Игоревич","course-6.jpg",["Пн, Ср, Пт · 19:00–20:30","Сб, Вс · 10:30–12:00"]),
        new(7,"Работа над ролью","Анализ персонажа, мотивации и драматической ситуации на материале пьесы.",0,16500,"Орлова Наталья Михайловна","course-7.jpg",["Вт, Чт · 18:30–20:00","Сб · 15:00–16:30"]),
        new(8,"Чтение пьесы вслух","Интонация, паузы и партнёрское чтение коротких драматических текстов.",1,10500,"Зайцев Павел Романович","course-8.jpg",["Ср, Пт · 19:00–20:30","Вс · 11:00–12:30"]),
        new(9,"Движение и ритм","Музыкальность, координация и командные упражнения на сцене.",2,11500,"Воронина Ксения Дмитриевна","course-9.jpg",["Пн, Чт · 18:00–19:30","Сб, Вс · 16:00–17:30"]),
        new(10,"Камерная сцена","От репетиций до показа: совместная работа над сценой в малой группе.",0,17500,"Кузнецов Роман Алексеевич","course-10.jpg",["Вт, Чт, Сб · 19:00–21:00","Вс · 13:00–15:00"]),
        new(11,"Выразительный рассказ","Структура истории, контакт со слушателем и выразительное слово.",1,12000,"Крылова Алина Станиславовна","course-11.jpg",["Пн, Ср · 20:00–21:30","Сб · 13:30–15:00"])
    ];

    private readonly string _connectionString;
    private readonly Dictionary<(int Course, int Group), int> _cart = [];
    private readonly HashSet<int> _favorites = [];
    private readonly StackPanel _content = new() { Spacing = 12 };
    private readonly TextBox _name = new() { PlaceholderText = "ФИО" };
    private readonly TextBox _phone = new() { PlaceholderText = "Телефон" };
    private readonly TextBox _email = new() { PlaceholderText = "Эл. почта" };
    private readonly ComboBox _payment = new() { ItemsSource = new[] { "Картой в студии", "Наличными в студии" }, SelectedIndex = 0 };
    private readonly Dictionary<string, Button> _navButtons = [];
    private Button? _cartHeaderButton;
    private TextBox? _promoInput;
    private string _promoCode = "";
    private bool _favoritesOnly;
    private readonly string _localStatePath;
    private string _page = "Каталог";

    public MainWindow()
    {
        Title = "СЦЕНА · Театральная студия";
        Width = 1320; Height = 860; MinWidth = 1020; MinHeight = 680; WindowState = WindowState.Maximized;
        Background = Brush("#F3EEE5");
        _connectionString = LoadConnectionString();
        _localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScenaStudio", "preferences.json");
        LoadPreferences();
        var layout = new Grid { ColumnDefinitions = new ColumnDefinitions("248,*"), RowDefinitions = new RowDefinitions("78,*") };
        var sidebar = new StackPanel { Spacing = 9, Margin = new Thickness(18, 20) };
        sidebar.Children.Add(new TextBlock { Text = "СЦЕНА", FontSize = 27, FontFamily = "Georgia", FontWeight = FontWeight.Bold, Foreground = Brush("#D3BC8D"), Margin = new Thickness(12, 8, 12, 0) });
        sidebar.Children.Add(new TextBlock { Text = "ТЕАТРАЛЬНЫЙ САЛОН · 2018", FontSize = 10, Foreground = Brush("#B5B5A0"), Margin = new Thickness(13, 0, 12, 26) });
        sidebar.Children.Add(new TextBlock { Text = "ПРОСТРАНСТВО", FontSize = 10, Foreground = Brush("#A8AE9F"), Margin = new Thickness(13, 2, 12, 3) });
        foreach (var item in new[] { "Главная", "Каталог", "Расписание", "Избранное", "Отзывы" })
        {
            AddNav(sidebar, item);
        }
        sidebar.Children.Add(new TextBlock { Text = "КАБИНЕТ", FontSize = 10, Foreground = Brush("#A8AE9F"), Margin = new Thickness(13, 20, 12, 3) });
        foreach (var item in new[] { "Корзина", "Личный кабинет", "Поддержка", "Администрирование" }) AddNav(sidebar, item);
        var studioCard = new Border { Margin = new Thickness(5, 34, 5, 0), Padding = new Thickness(15), CornerRadius = new CornerRadius(12), Background = Brush("#263A30"), Child = new StackPanel { Spacing = 6, Children = { new TextBlock { Text = "ВАШЕ ВРЕМЯ — СЦЕНА", FontSize = 10, Foreground = Brush("#D3BC8D") }, new TextBlock { Text = "Камерные группы.\nБольшие открытия.", FontFamily = "Georgia", FontSize = 16, Foreground = Brush("#F5EFE3"), TextWrapping = TextWrapping.Wrap } } } };
        sidebar.Children.Add(studioCard);
        var side = new Border { Background = Gradient("#14251D", "#263A30", 110), Child = new ScrollViewer { Content = sidebar } };
        Grid.SetRowSpan(side, 2); layout.Children.Add(side);
        var top = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"), Margin = new Thickness(34, 14, 38, 12) };
        top.Children.Add(new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 1, Children = { new TextBlock { Text = "САНКТ-ПЕТЕРБУРГ  ·  ПЕТРОГРАДСКАЯ", FontSize = 10, Foreground = Brush("#827C6C") }, new TextBlock { Text = "Дом, где раскрывается голос", FontFamily = "Georgia", FontSize = 17, Foreground = Brush("#20352A") } } });
        var cartTop = MakeButton("Корзина  ·  " + CartCount(), false); _cartHeaderButton = cartTop; cartTop.Margin = new Thickness(8, 3); cartTop.Click += (_, _) => Navigate("Корзина"); Grid.SetColumn(cartTop, 1); top.Children.Add(cartTop);
        var profileTop = MakeButton("Личный кабинет  ↗", true); profileTop.Margin = new Thickness(8, 3); profileTop.Click += (_, _) => Navigate("Личный кабинет"); Grid.SetColumn(profileTop, 2); top.Children.Add(profileTop);
        Grid.SetColumn(top, 1); layout.Children.Add(top);
        var scroll = new ScrollViewer { Content = _content, Margin = new Thickness(34, 16, 38, 34), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetColumn(scroll, 1); Grid.SetRow(scroll, 1); layout.Children.Add(scroll); Content = layout;
        Navigate("Главная");
    }

    private static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));
    private static IBrush Gradient(string first, string second, double angle = 45) => new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops { new GradientStop(Color.Parse(first), 0), new GradientStop(Color.Parse(second), 1) }
    };
    private Button MakeButton(string label, bool primary)
    {
        var b = new Button { Content = label, Padding = new Thickness(16, 10), CornerRadius = new CornerRadius(7), FontSize = 13,
            Background = primary ? Brush("#A88650") : Brush("#E8E0D1"), Foreground = primary ? Brush("#FFFDF7") : Brush("#27392F"), BorderThickness = new Thickness(0) };
        return b;
    }
    private void AddNav(StackPanel sidebar, string label)
    {
        var caption = label switch { "Главная" => "⌂   Главная", "Каталог" => "◇   Каталог", "Расписание" => "▦   Расписание", "Избранное" => "♡   Избранное", "Отзывы" => "✦   Отзывы", "Корзина" => "▱   Корзина", "Личный кабинет" => "◉   Личный кабинет", "Поддержка" => "✉   Поддержка", _ => "⚙   Администрирование" };
        var button = new Button { Content = caption, HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(14, 12), Background = Brushes.Transparent, Foreground = Brush("#E9E6D9"), BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(7), FontSize = 13 };
        button.Click += (_, _) => Navigate(label); _navButtons[label] = button; sidebar.Children.Add(button);
    }
    private int CartCount() => _cart.Values.Sum();
    private void LoadPreferences()
    {
        try { var saved = JsonDocument.Parse(File.ReadAllText(_localStatePath)).RootElement; if (saved.TryGetProperty("favorites", out var favs)) foreach (var f in favs.EnumerateArray()) _favorites.Add(f.GetInt32()); if (saved.TryGetProperty("cart", out var items)) foreach (var item in items.EnumerateArray()) _cart[(item.GetProperty("course").GetInt32(), item.GetProperty("group").GetInt32())] = item.GetProperty("quantity").GetInt32(); if (saved.TryGetProperty("name", out var n)) _name.Text = n.GetString(); if (saved.TryGetProperty("phone", out var p)) _phone.Text = p.GetString(); if (saved.TryGetProperty("email", out var e)) _email.Text = e.GetString(); }
        catch { }
    }
    private void SavePreferences()
    {
        try { var dir = Path.GetDirectoryName(_localStatePath)!; Directory.CreateDirectory(dir); File.WriteAllText(_localStatePath, JsonSerializer.Serialize(new { favorites = _favorites, cart = _cart.Select(x => new { course = x.Key.Course, group = x.Key.Group, quantity = x.Value }), name = _name.Text, phone = _phone.Text, email = _email.Text })); }
        catch { }
    }
    private static string LoadConnectionString()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            return JsonDocument.Parse(File.ReadAllText(path)).RootElement.GetProperty("ConnectionStrings").GetProperty("Studio").GetString()!;
        }
        catch { return @"Server=(localdb)\MSSQLLocalDB;Database=ScenaStudio;Trusted_Connection=True;TrustServerCertificate=True;"; }
    }
    private StudioDbContext Db() => new(new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(_connectionString).Options);
    private async Task EnsureDatabase()
    {
        await using var db = Db(); await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL CREATE TABLE dbo.Reviews (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY, Name nvarchar(100) NOT NULL, Text nvarchar(2000) NOT NULL, Rating int NOT NULL, CreatedAt datetime2 NOT NULL)");
    }
    private void Navigate(string page)
    {
        _page = page; _content.Children.Clear();
        foreach (var (name, button) in _navButtons) { button.Background = name == page ? Brush("#A88650") : Brushes.Transparent; button.Foreground = Brush("#F4F0E5"); }
        switch (page)
        {
            case "Главная": ShowHome(); break;
            case "Каталог": _favoritesOnly = false; ShowCatalog(); break;
            case "Расписание": ShowSchedule(); break;
            case "Избранное": _favoritesOnly = true; ShowCatalog(); break;
            case "Отзывы": ShowReviews(); break;
            case "Корзина": ShowCart(); break;
            case "Личный кабинет": ShowAccount(); break;
            case "Поддержка": ShowSupport(); break;
            default: ShowAdmin(); break;
        }
    }
    private void Heading(string title, string? subtitle = null)
    {
        _content.Children.Add(new TextBlock { Text = title, FontFamily = "Georgia", FontSize = 31, FontWeight = FontWeight.SemiBold, Foreground = Brush("#20352A") });
        if (subtitle != null) _content.Children.Add(new TextBlock { Text = subtitle, FontSize = 14, Foreground = Brush("#827C6C"), TextWrapping = TextWrapping.Wrap });
    }
    private Control Surface(Control content, Thickness? padding = null) => new Border { Background = Brush("#FFFCF6"), BorderBrush = Brush("#E3D9C8"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = padding ?? new Thickness(18), Child = content };
    private void ShowHome()
    {
        Heading("Добро пожаловать в «Сцену»", "ТЕАТРАЛЬНОЕ ИСКУССТВО · КАМЕРНЫЕ ГРУППЫ · ПЕТРОГРАДСКАЯ СТОРОНА");
        var hero = new Grid { ColumnDefinitions = new ColumnDefinitions("1.15*,0.85*"), MinHeight = 310, ClipToBounds = true };
        var copy = new StackPanel { Margin = new Thickness(32), VerticalAlignment = VerticalAlignment.Center, Spacing = 15 };
        copy.Children.Add(new TextBlock { Text = "ИСКУССТВО\nБЫТЬ СОБОЙ", FontFamily = "Georgia", FontSize = 38, FontWeight = FontWeight.SemiBold, Foreground = Brush("#F6F0E2"), TextWrapping = TextWrapping.Wrap });
        copy.Children.Add(new TextBlock { Text = "Место, где бережно раскрывают голос,\nхарактер и сценическую свободу.", FontSize = 16, Foreground = Brush("#D8D7C6"), TextWrapping = TextWrapping.Wrap });
        var cta = MakeButton("Выбрать занятие  →", true); cta.HorizontalAlignment = HorizontalAlignment.Left; cta.Click += (_, _) => Navigate("Каталог"); copy.Children.Add(cta);
        hero.Children.Add(new Border { Background = Gradient("#12241C", "#4B513A"), Child = copy });
        var heroImage = CourseImage("studio.jpg"); heroImage.HorizontalAlignment = HorizontalAlignment.Stretch; heroImage.VerticalAlignment = VerticalAlignment.Stretch; heroImage.Width = double.NaN; heroImage.Height = double.NaN; Grid.SetColumn(heroImage, 1); hero.Children.Add(heroImage);
        _content.Children.Add(new Border { CornerRadius = new CornerRadius(16), ClipToBounds = true, Child = hero });
        var stats = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*"), ColumnSpacing = 12, Margin = new Thickness(0, 3) };
        var statTexts = new[] { ("12", "авторских курсов"), ("8", "занятий в программе"), ("до 10", "человек в группе"), ("с 2018", "создаём сцену") };
        for (var i = 0; i < statTexts.Length; i++) { var (value, label) = statTexts[i]; var stat = Surface(new StackPanel { Spacing = 4, Children = { new TextBlock { Text = value, FontFamily = "Georgia", FontSize = 24, Foreground = Brush("#8B6B3F") }, new TextBlock { Text = label, FontSize = 12, Foreground = Brush("#827C6C") } } }, new Thickness(16, 13)); Grid.SetColumn(stat, i); stats.Children.Add(stat); }
        _content.Children.Add(stats);
        SectionLabel("ВЫБОР ХУДОЖЕСТВЕННОГО РУКОВОДИТЕЛЯ", "Курсы, с которых начинается большое увлечение");
        var chosen = new WrapPanel { Orientation = Orientation.Horizontal, ItemWidth = 335, ItemHeight = 475, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var c in Courses.Take(3)) chosen.Children.Add(CourseCard(c)); _content.Children.Add(chosen);
        var quote = new Border { Background = Gradient("#E9DDC8", "#F4EFE5"), CornerRadius = new CornerRadius(12), Padding = new Thickness(25), Child = new StackPanel { Spacing = 6, Children = { new TextBlock { Text = "«Театр начинается там, где человек впервые позволяет себе быть услышанным»", FontFamily = "Georgia", FontSize = 19, FontStyle = FontStyle.Italic, Foreground = Brush("#334638"), TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = "— ИЗ МАНИФЕСТА СТУДИИ", FontSize = 10, Foreground = Brush("#8B6B3F") } } } }; _content.Children.Add(quote);
    }
    private void SectionLabel(string label, string title)
    {
        _content.Children.Add(new TextBlock { Text = label, FontSize = 10, Foreground = Brush("#9B7C4F"), Margin = new Thickness(2, 12, 0, 0) });
        _content.Children.Add(new TextBlock { Text = title, FontFamily = "Georgia", FontSize = 24, Foreground = Brush("#20352A"), Margin = new Thickness(0, -6, 0, 4) });
    }
    private void ShowCatalog()
    {
        Heading(_favoritesOnly ? "Ваше избранное" : "Коллекция занятий", _favoritesOnly ? "Курсы, которые вы сохранили для себя." : "Выберите направление. Остальное мы подскажем на первой встрече.");
        var search = new TextBox { PlaceholderText = "⌕  Название курса, навык или педагог", Padding = new Thickness(14), FontSize = 14 };
        var category = new ComboBox { ItemsSource = new[] { "Все направления" }.Concat(Categories).ToArray(), SelectedIndex = 0, MinWidth = 220, Padding = new Thickness(12) };
        var sort = new ComboBox { ItemsSource = new[] { "Популярные", "Сначала дешевле", "Сначала дороже", "По алфавиту" }, SelectedIndex = 0, MinWidth = 180, Padding = new Thickness(12) };
        var controls = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220,190"), ColumnSpacing = 10 }; controls.Children.Add(search); Grid.SetColumn(category, 1); controls.Children.Add(category); Grid.SetColumn(sort, 2); controls.Children.Add(sort); _content.Children.Add(controls);
        var countLabel = new TextBlock { FontSize = 12, Foreground = Brush("#817868"), Margin = new Thickness(2, 0, 0, 0) }; _content.Children.Add(countLabel);
        var list = new WrapPanel { Orientation = Orientation.Horizontal, ItemWidth = 335, ItemHeight = 475, HorizontalAlignment = HorizontalAlignment.Left }; _content.Children.Add(list);
        void Render()
        {
            list.Children.Clear();
            var q = search.Text?.Trim() ?? ""; var cat = category.SelectedIndex - 1;
            var matching = Courses.Where(c => (cat < 0 || c.Category == cat) && (c.Title + c.Description + c.Teacher + Categories[c.Category]).Contains(q, StringComparison.CurrentCultureIgnoreCase) && (!_favoritesOnly || _favorites.Contains(c.Id)));
            matching = sort.SelectedIndex switch { 1 => matching.OrderBy(c => c.Price), 2 => matching.OrderByDescending(c => c.Price), 3 => matching.OrderBy(c => c.Title), _ => matching.OrderBy(c => c.Id) };
            var results = matching.ToArray(); countLabel.Text = $"{results.Length} КУРСОВ В КОЛЛЕКЦИИ  ·  КАМЕРНЫЕ ГРУППЫ ДО 10 ЧЕЛОВЕК";
            foreach (var course in results)
            {
                list.Children.Add(CourseCard(course));
            }
            if (list.Children.Count == 0) list.Children.Add(Surface(new TextBlock { Text = _favoritesOnly ? "Избранное пока пусто. Отметьте сердечком курсы, которые хотите сохранить." : "По вашему запросу курсы не найдены.", TextWrapping = TextWrapping.Wrap }, new Thickness(18)));
        }
        search.TextChanged += (_, _) => Render(); category.SelectionChanged += (_, _) => Render(); sort.SelectionChanged += (_, _) => Render(); Render();
    }
    private Control CourseCard(Course course)
    {
        var card = new StackPanel { Spacing = 9 };
        var image = CourseImage(course.Image); image.Width = 305; image.Height = 166; card.Children.Add(new Border { CornerRadius = new CornerRadius(8), ClipToBounds = true, Child = image });
        var top = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        top.Children.Add(new TextBlock { Text = Categories[course.Category].ToUpperInvariant(), FontSize = 9, Foreground = Brush("#98794C"), VerticalAlignment = VerticalAlignment.Center });
        var heart = MakeButton(_favorites.Contains(course.Id) ? "♥" : "♡", false); heart.Padding = new Thickness(9, 5); heart.Click += (_, _) => { if (!_favorites.Add(course.Id)) _favorites.Remove(course.Id); SavePreferences(); heart.Content = _favorites.Contains(course.Id) ? "♥" : "♡"; heart.Foreground = _favorites.Contains(course.Id) ? Brush("#7B3944") : Brush("#27392F"); if (_favoritesOnly && !_favorites.Contains(course.Id)) ShowCatalog(); };
        Grid.SetColumn(heart, 1); top.Children.Add(heart); card.Children.Add(top);
        card.Children.Add(new TextBlock { Text = course.Title, FontFamily = "Georgia", FontSize = 20, Foreground = Brush("#23382D"), TextWrapping = TextWrapping.Wrap, MinHeight = 52 });
        card.Children.Add(new TextBlock { Text = course.Description, FontSize = 12, Foreground = Brush("#777365"), TextWrapping = TextWrapping.Wrap, MinHeight = 50 });
        card.Children.Add(new TextBlock { Text = "ПЕДАГОГ  ·  " + course.Teacher, FontSize = 10, Foreground = Brush("#827C6C"), TextWrapping = TextWrapping.Wrap });
        card.Children.Add(new Border { Height = 1, Background = Brush("#E7DDCC"), Margin = new Thickness(0, 3) });
        card.Children.Add(new TextBlock { Text = "от " + Money(course.Price) + "  ·  8 встреч", FontFamily = "Georgia", FontSize = 17, Foreground = Brush("#8B6B3F") });
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var details = MakeButton("О курсе", false); details.Click += async (_, _) => await OpenCourse(course); actions.Children.Add(details);
        var add = MakeButton("Записаться", true); add.Click += (_, _) => AddToCart(course, 0); actions.Children.Add(add); card.Children.Add(actions);
        return new Border { Width = 325, Height = 458, Margin = new Thickness(0, 2, 14, 14), Padding = new Thickness(10), Background = Brush("#FFFCF6"), BorderBrush = Brush("#E4DAC9"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Child = card };
    }
    private async Task OpenCourse(Course course)
    {
        var dialog = new Window { Title = course.Title, Width = 700, SizeToContent = SizeToContent.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = Brush("#F3EEE5"), CanResize = false };
        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 12 };
        var img = CourseImage(course.Image); img.Width = 640; img.Height = 220; panel.Children.Add(new Border { CornerRadius = new CornerRadius(10), ClipToBounds = true, Child = img });
        panel.Children.Add(new TextBlock { Text = Categories[course.Category].ToUpperInvariant(), FontSize = 10, Foreground = Brush("#9B7C4F") });
        panel.Children.Add(new TextBlock { Text = course.Title, FontFamily = "Georgia", FontSize = 27, Foreground = Brush("#20352A"), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = course.Description + "\n\nВосемь встреч продолжительностью 90 минут. Предыдущий опыт не обязателен. Занятия проходят в малой группе, с вниманием к каждому участнику.", TextWrapping = TextWrapping.Wrap, Foreground = Brush("#655F52") });
        panel.Children.Add(new TextBlock { Text = "Педагог: " + course.Teacher + "   ·   Стоимость программы: " + Money(course.Price), Foreground = Brush("#8B6B3F") });
        foreach (var (schedule, index) in course.Schedule.Select((s, i) => (s, i)))
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 2) };
            row.Children.Add(new TextBlock { Text = $"Группа {index + 1}  ·  {schedule}", VerticalAlignment = VerticalAlignment.Center });
            var choose = MakeButton("Выбрать группу", true); choose.Click += (_, _) => { AddToCart(course, index); dialog.Close(); }; Grid.SetColumn(choose, 1); row.Children.Add(choose); panel.Children.Add(Surface(row, new Thickness(11)));
        }
        dialog.Content = panel; await dialog.ShowDialog(this);
    }
    private void ShowSchedule()
    {
        Heading("Расписание занятий", "Две группы на каждом направлении. Восемь встреч по 90 минут.");
        foreach (var c in Courses) for (var g = 0; g < 2; g++)
            _content.Children.Add(Card($"{c.Schedule[g]} · {c.Title}\n{c.Teacher} · {Money(c.Price)}", () => AddToCart(c, g)));
    }
    private void AddToCart(Course course, int group)
    {
        _cart[(course.Id, group)] = _cart.GetValueOrDefault((course.Id, group)) + 1; SavePreferences(); UpdateCartLabel();
        Message("Добавлено в корзину", course.Title + " · " + course.Schedule[group]);
    }
    private void UpdateCartLabel() { if (_cartHeaderButton != null) _cartHeaderButton.Content = "Корзина  ·  " + CartCount(); }
    private void ShowCart()
    {
        Heading("Ваша корзина", "Участие в программе подтверждается после оформления заявки.");
        if (_cart.Count == 0)
        {
            var empty = new StackPanel { Spacing = 10 }; empty.Children.Add(new TextBlock { Text = "Корзина пока пуста", FontFamily = "Georgia", FontSize = 22, Foreground = Brush("#20352A") }); empty.Children.Add(new TextBlock { Text = "Посмотрите коллекцию занятий и выберите группу, которая вам подходит." });
            var browse = MakeButton("Перейти в каталог", true); browse.HorizontalAlignment = HorizontalAlignment.Left; browse.Click += (_, _) => Navigate("Каталог"); empty.Children.Add(browse);
            _content.Children.Add(Surface(empty, new Thickness(20))); return;
        }
        var total = 0;
        foreach (var entry in _cart.ToArray())
        {
            var c = Courses[entry.Key.Course]; total += c.Price * entry.Value;
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto"), ColumnSpacing = 8 };
            row.Children.Add(new StackPanel { Spacing = 4, Children = { new TextBlock { Text = c.Title, FontFamily = "Georgia", FontSize = 17, Foreground = Brush("#20352A"), TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = $"Группа {entry.Key.Group + 1} · {c.Schedule[entry.Key.Group]} · {Money(c.Price)} за программу", FontSize = 12, Foreground = Brush("#827C6C"), TextWrapping = TextWrapping.Wrap } } });
            var minus = MakeButton("−", false); Grid.SetColumn(minus, 1); minus.Click += (_, _) => { if (entry.Value <= 1) _cart.Remove(entry.Key); else _cart[entry.Key]--; SavePreferences(); ShowCart(); UpdateCartLabel(); }; row.Children.Add(minus);
            var qty = new TextBlock { Text = entry.Value.ToString(), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0) }; Grid.SetColumn(qty, 2); row.Children.Add(qty);
            var plus = MakeButton("+", false); Grid.SetColumn(plus, 3); plus.Click += (_, _) => { _cart[entry.Key]++; SavePreferences(); ShowCart(); UpdateCartLabel(); }; row.Children.Add(plus);
            var remove = MakeButton("Убрать", false); Grid.SetColumn(remove, 4); remove.Click += (_, _) => { _cart.Remove(entry.Key); SavePreferences(); ShowCart(); UpdateCartLabel(); }; row.Children.Add(remove);
            _content.Children.Add(Surface(row));
        }
        var promoRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        _promoInput = new TextBox { PlaceholderText = "Промокод", Text = _promoCode, Padding = new Thickness(12) }; promoRow.Children.Add(_promoInput);
        var apply = MakeButton("Применить", false); apply.Click += (_, _) => { var code = _promoInput.Text?.Trim().ToUpperInvariant() ?? ""; if (code is "SCENA10" or "DEBUT") { _promoCode = code; Message("Промокод применён", code == "SCENA10" ? "Ваша скидка — 10%." : "Ваша скидка — 5%."); } else { _promoCode = ""; Message("Промокод не найден", "Проверьте написание или продолжите без промокода."); } ShowCart(); }; Grid.SetColumn(apply, 1); promoRow.Children.Add(apply); _content.Children.Add(Surface(promoRow, new Thickness(12)));
        var discount = _promoCode == "SCENA10" ? 10 : _promoCode == "DEBUT" ? 5 : 0; var payable = total - total * discount / 100;
        var summary = new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "ИТОГО К ОПЛАТЕ В СТУДИИ", FontSize = 10, Foreground = Brush("#827C6C") }, new TextBlock { Text = discount > 0 ? $"{Money(payable)}   ·   скидка {discount}% (вместо {Money(total)})" : Money(total), FontFamily = "Georgia", FontSize = 25, Foreground = Brush("#805E37") }, new TextBlock { Text = "Онлайн-оплата не требуется. Оплатить можно на первом занятии.", FontSize = 12, Foreground = Brush("#827C6C") } } };
        _content.Children.Add(Surface(summary));
        Heading("Контакт для подтверждения");
        foreach (var field in new[] { _name, _phone, _email }) { field.Padding = new Thickness(12); _content.Children.Add(field); }
        _content.Children.Add(_payment);
        var submit = MakeButton("Подтвердить запись", true); submit.Click += async (_, _) => await Checkout(payable); _content.Children.Add(submit);
    }
    private async Task Checkout(int total)
    {
        if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_phone.Text) || string.IsNullOrWhiteSpace(_email.Text) || !(_email.Text?.Contains('@') ?? false)) { Message("Проверьте данные", "Укажите ФИО, телефон и корректный адрес электронной почты."); return; }
        try
        {
            await EnsureDatabase(); await using var db = Db();
            var phone = _phone.Text!.Trim(); var customer = await db.Customers.FirstOrDefaultAsync(x => x.Phone == phone);
            if (customer == null) { customer = new Customer { Name = _name.Text!.Trim(), Phone = phone, Email = _email.Text!.Trim() }; db.Customers.Add(customer); }
            else { customer.Name = _name.Text!.Trim(); customer.Email = _email.Text!.Trim(); }
            await db.SaveChangesAsync();
            var discount = _promoCode == "SCENA10" ? 10 : _promoCode == "DEBUT" ? 5 : 0;
            foreach (var (key, quantity) in _cart) { var sum = Courses[key.Course].Price * quantity; db.Enrollments.Add(new Enrollment { CustomerId = customer.Id, CourseId = key.Course, GroupNumber = key.Group, Quantity = quantity, Total = sum - sum * discount / 100, PaymentMethod = _payment.SelectedItem?.ToString() ?? "Картой в студии" }); }
            await db.SaveChangesAsync(); _cart.Clear(); _promoCode = ""; SavePreferences(); UpdateCartLabel(); Message("Заявка принята", $"Заявка на {Money(total)} сохранена в Microsoft SQL Server. Приходите за 10–15 минут до начала."); Navigate("Личный кабинет");
        }
        catch (Exception ex) { Message("Не удалось сохранить заявку", DbError(ex)); }
    }
    private void ShowAccount()
    {
        Heading("Личный кабинет", "История ваших заявок и выбранные программы.");
        _content.Children.Add(Surface(new StackPanel { Spacing = 9, Children = { new TextBlock { Text = "Найдите свои записи по номеру телефона", FontFamily = "Georgia", FontSize = 18, Foreground = Brush("#20352A") }, _phone, MakeButton("Показать историю", true) } }));
        var lookup = (Button)((StackPanel)((Border)_content.Children[^1]).Child!).Children[2]; lookup.Click += async (_, _) => await LoadAccount(_phone.Text?.Trim() ?? "");
        if (!string.IsNullOrWhiteSpace(_phone.Text)) _ = LoadAccount(_phone.Text.Trim());
    }
    private async Task LoadAccount(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) { _content.Children.Add(new TextBlock { Text = "Введите телефон, указанный при записи.", Foreground = Brush("#827C6C") }); return; }
        try
        {
            await EnsureDatabase(); await using var db = Db();
            var list = await db.Enrollments.Include(x => x.Customer).Where(x => x.Customer!.Phone == phone).OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync();
            if (_page != "Личный кабинет") return;
            if (list.Count == 0) _content.Children.Add(Surface(new TextBlock { Text = "По этому номеру пока нет заявок.", Foreground = Brush("#827C6C") }));
            foreach (var o in list)
            {
                var c = Courses.ElementAtOrDefault(o.CourseId); if (c == null) continue;
                _content.Children.Add(Card($"Заявка №{o.Id} · {o.Status} · {Money(o.Total)}\n{o.Customer?.Name} · {o.Customer?.Phone}\n{c.Title} · группа {o.GroupNumber + 1} · {o.CreatedAt:g}\nОплата: {o.PaymentMethod}", async () =>
                {
                    if (o.Status is "Новый" or "Оплачен") { o.Status = "Отменён"; await db.SaveChangesAsync(); Navigate("Личный кабинет"); }
                }));
            }
        }
        catch (Exception ex) { _content.Children.Add(new TextBlock { Text = DbError(ex), TextWrapping = TextWrapping.Wrap }); }
    }
    private void ShowReviews()
    {
        Heading("Голоса студии", "Каждый путь на сцену начинается с доверия.");
        var name = new TextBox { PlaceholderText = "Ваше имя", Text = _name.Text, Padding = new Thickness(12) };
        var rating = new ComboBox { ItemsSource = new[] { "5 · Великолепно", "4 · Очень хорошо", "3 · Хорошо", "2 · Есть над чем работать", "1 · Не понравилось" }, SelectedIndex = 0, Padding = new Thickness(12) };
        var text = new TextBox { PlaceholderText = "Поделитесь впечатлением о занятии", MinHeight = 90, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Padding = new Thickness(12) };
        var send = MakeButton("Оставить отзыв", true);
        _content.Children.Add(Surface(new StackPanel { Spacing = 10, Children = { new TextBlock { Text = "Ваша история", FontFamily = "Georgia", FontSize = 20, Foreground = Brush("#20352A") }, name, rating, text, send } }));
        send.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(text.Text) || text.Text.Trim().Length < 5) { Message("Заполните форму", "Укажите имя и отзыв длиной не менее пяти символов."); return; }
            try { await EnsureDatabase(); await using var db = Db(); db.Reviews.Add(new StudioReview { Name = name.Text.Trim(), Text = text.Text.Trim(), Rating = 5 - rating.SelectedIndex }); await db.SaveChangesAsync(); _name.Text = name.Text; SavePreferences(); Navigate("Отзывы"); }
            catch (Exception ex) { Message("Не удалось сохранить", DbError(ex)); }
        };
        _ = LoadReviews();
    }
    private async Task LoadReviews()
    {
        try
        {
            await EnsureDatabase(); await using var db = Db();
            var reviews = await db.Reviews.OrderByDescending(x => x.CreatedAt).Take(30).ToListAsync();
            if (_page != "Отзывы") return;
            foreach (var review in reviews)
                _content.Children.Add(Surface(new StackPanel { Spacing = 7, Children = { new TextBlock { Text = new string('★', Math.Clamp(review.Rating, 1, 5)) + new string('☆', 5 - Math.Clamp(review.Rating, 1, 5)), Foreground = Brush("#A88650"), FontSize = 16 }, new TextBlock { Text = "“" + review.Text + "”", FontFamily = "Georgia", FontSize = 17, Foreground = Brush("#304437"), TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = review.Name + "  ·  " + review.CreatedAt.ToString("d MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")), FontSize = 11, Foreground = Brush("#827C6C") } } }));
        }
        catch (Exception ex) { _content.Children.Add(new TextBlock { Text = DbError(ex), TextWrapping = TextWrapping.Wrap }); }
    }
    private void ShowSupport()
    {
        Heading("Поддержка", "Напишите вопрос — обращение сохранится в базе данных.");
        var person = new TextBox { PlaceholderText = "Ваше имя" }; var message = new TextBox { PlaceholderText = "Ваш вопрос", AcceptsReturn = true, MinHeight = 110, TextWrapping = TextWrapping.Wrap };
        _content.Children.Add(person); _content.Children.Add(message);
        var send = new Button { Content = "Отправить обращение" };
        send.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Trim().Length < 5) { Message("Введите вопрос", "Минимальная длина — 5 символов."); return; }
            try { await EnsureDatabase(); await using var db = Db(); db.SupportMessages.Add(new SupportMessage { CustomerName = string.IsNullOrWhiteSpace(person.Text) ? "Гость" : person.Text.Trim(), Text = message.Text.Trim() }); await db.SaveChangesAsync(); message.Text = ""; Message("Отправлено", "Мы ответим вам в ближайшее время."); }
            catch (Exception ex) { Message("Не удалось отправить", DbError(ex)); }
        }; _content.Children.Add(send);
        _ = LoadMessages();
    }
    private async Task LoadMessages()
    {
        try
        {
            await EnsureDatabase(); await using var db = Db();
            var messages = await db.SupportMessages.OrderByDescending(x => x.CreatedAt).Take(30).ToListAsync();
            if (_page != "Поддержка") return;
            foreach (var m in messages)
                _content.Children.Add(Card($"{m.CustomerName} · {m.CreatedAt:g}\n{m.Text}\n{(string.IsNullOrEmpty(m.Reply) ? "Ответ ожидается" : "Ответ: " + m.Reply)}"));
        }
        catch { }
    }
    private void ShowAdmin()
    {
        Heading("Администрирование", "Для демонстрации курсовой: просмотр заявок и смена статуса."); _ = LoadAdmin();
    }
    private async Task LoadAdmin()
    {
        try
        {
            await EnsureDatabase(); await using var db = Db();
            var orders = await db.Enrollments.Include(x => x.Customer).OrderByDescending(x => x.CreatedAt).ToListAsync();
            if (_page != "Администрирование") return;
            foreach (var o in orders)
            {
                var c = Courses.ElementAtOrDefault(o.CourseId); if (c == null) continue;
                var row = new StackPanel { Spacing = 5 };
                row.Children.Add(new TextBlock { Text = $"№{o.Id} · {o.Customer?.Name} · {o.Customer?.Phone} · {c.Title} · группа {o.GroupNumber + 1} · {Money(o.Total)} · {o.Status}", TextWrapping = TextWrapping.Wrap });
                var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                foreach (var status in new[] { "Оплачен", "Завершён", "Отменён" }) { var b = new Button { Content = status }; b.Click += async (_, _) => { o.Status = status; await db.SaveChangesAsync(); Navigate("Администрирование"); }; actions.Children.Add(b); }
                row.Children.Add(actions); _content.Children.Add(new Border { Background = Brushes.White, Padding = new Thickness(12), Child = row });
            }
            var title = new TextBlock { Text = "Обращения", FontSize = 22, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 14, 0, 0) }; _content.Children.Add(title);
            var messages = await db.SupportMessages.OrderByDescending(x => x.CreatedAt).ToListAsync();
            if (_page != "Администрирование") return;
            foreach (var m in messages)
            {
                var reply = new TextBox { PlaceholderText = "Ответ", Text = m.Reply };
                var save = new Button { Content = "Сохранить ответ" }; save.Click += async (_, _) => { m.Reply = reply.Text?.Trim() ?? ""; await db.SaveChangesAsync(); Message("Сохранено", "Ответ добавлен."); };
                _content.Children.Add(new TextBlock { Text = $"{m.CustomerName}: {m.Text}", TextWrapping = TextWrapping.Wrap }); _content.Children.Add(reply); _content.Children.Add(save);
            }
        }
        catch (Exception ex) { _content.Children.Add(new TextBlock { Text = DbError(ex), TextWrapping = TextWrapping.Wrap }); }
    }
    private Control Card(string text, Action? action = null)
    {
        var panel = new StackPanel { Spacing = 9 }; panel.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap });
        if (action != null) { var button = new Button { Content = _page == "Личный кабинет" ? "Отменить заявку" : "Добавить в корзину", HorizontalAlignment = HorizontalAlignment.Left }; button.Click += (_, _) => action(); panel.Children.Add(button); }
        return Surface(panel, new Thickness(15));
    }
    private static Control CourseImage(string file)
    {
        try { return new Image { Source = new Bitmap(Path.Combine(AppContext.BaseDirectory, "assets", file)), Width = 150, Height = 120, Stretch = Stretch.UniformToFill }; }
        catch { return new Border { Width = 150, Height = 120, Background = Brush("#DDD4C7"), Child = new TextBlock { Text = "Сцена", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center } }; }
    }
    private static string Money(int value) => value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")) + " ₽";
    private static string DbError(Exception ex) => "Не удалось подключиться к Microsoft SQL Server. Проверьте, что SQL Server LocalDB установлен и запущен, а строка подключения верна.\n\n" + ex.GetBaseException().Message;
    private async void Message(string title, string body)
    {
        var dialog = new Window { Title = title, Width = 470, SizeToContent = SizeToContent.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner, CanResize = false, Background = Brush("#F3EEE5") };
        var ok = MakeButton("Понятно", true); ok.HorizontalAlignment = HorizontalAlignment.Right;
        ok.Click += (_, _) => dialog.Close();
        dialog.Content = Surface(new StackPanel { Spacing = 14, Children = { new TextBlock { Text = title, FontFamily = "Georgia", FontSize = 20, Foreground = Brush("#20352A") }, new TextBlock { Text = body, TextWrapping = TextWrapping.Wrap }, ok } }, new Thickness(20)); dialog.Padding = new Thickness(16);
        await dialog.ShowDialog(this);
    }
}


