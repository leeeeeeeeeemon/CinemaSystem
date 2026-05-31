// Views/ClientSellTicketWindow.xaml.cs
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class ClientSellTicketWindow : Window
    {
        private readonly Session _session;
        private Visitor? _foundVisitor = null; // найденный по бонусной карте посетитель

        public ClientSellTicketWindow(Session session)
        {
            InitializeComponent();
            _session = session;
            LoadData();
        }

        private void LoadData()
        {
            txtFilmTitle.Text = _session.Film.Title;
            txtSessionInfo.Text = $"Зал {_session.Hall.HallNumber}  •  {_session.StartDateTime:dd.MM.yyyy HH:mm}";
            txtBasePrice.Text = $"Базовая цена: {_session.BasePrice} руб. за билет";

            // Заполняем количество по числу свободных мест (максимум 5)
            int maxTickets = Math.Min(_session.AvailableSeats, 5);
            cmbQuantity.Items.Clear();
            for (int i = 1; i <= maxTickets; i++)
                cmbQuantity.Items.Add(new ComboBoxItem { Content = i.ToString() });

            cmbQuantity.SelectedIndex = 0;
            UpdatePrice();
        }

        // Когда вводят номер бонусной карты — ищем посетителя в БД
        private void txtBonusCard_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cardNumber = txtBonusCard.Text.Trim();

            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                _foundVisitor = null;
                txtBonusInfo.Visibility = Visibility.Collapsed;
                UpdatePrice();
                return;
            }

            using (var db = new CinemaDbContext())
            {
                _foundVisitor = db.Visitors
                    .FirstOrDefault(v => v.BonusCardNumber == cardNumber);
            }

            if (_foundVisitor != null)
            {
                txtBonusInfo.Text = $"✓ Карта найдена: {_foundVisitor.FullName}  •  Баллов: {_foundVisitor.BonusPoints}";
                txtBonusInfo.Visibility = Visibility.Visible;
            }
            else
            {
                txtBonusInfo.Text = "Карта не найдена — скидка не применяется";
                txtBonusInfo.Foreground = System.Windows.Media.Brushes.OrangeRed;
                txtBonusInfo.Visibility = Visibility.Visible;
            }

            UpdatePrice();
        }

        private void txtFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Просто пересчитываем цену (на случай если ФИО влияет на UI-состояние)
            UpdatePrice();
        }

        private void cmbQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }

        private void UpdatePrice()
        {
            if (cmbQuantity.SelectedItem is not ComboBoxItem item) return;

            int quantity = int.Parse(item.Content.ToString()!);
            decimal discountPercent = GetDiscountPercent();
            decimal pricePerTicket = _session.BasePrice * (1 - discountPercent);
            decimal total = pricePerTicket * quantity;

            if (discountPercent > 0)
                txtDiscountInfo.Text = $"Скидка по бонусной карте: {discountPercent * 100:F0}%  (-{_session.BasePrice * discountPercent * quantity:F0} руб.)";
            else
                txtDiscountInfo.Text = "Скидка: нет";

            txtFinalPrice.Text = $"ИТОГО за {quantity} билет(ов): {total:F0} руб.";
        }

        private decimal GetDiscountPercent()
        {
            if (_foundVisitor == null) return 0;

            if (_foundVisitor.BonusPoints >= 300) return 0.15m;
            if (_foundVisitor.BonusPoints >= 100) return 0.10m;
            return 0.05m;
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация ФИО
            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
            {
                MessageBox.Show("Введите ваше ФИО!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            int quantity = int.Parse(((ComboBoxItem)cmbQuantity.SelectedItem).Content.ToString()!);
            decimal discountPercent = GetDiscountPercent();
            decimal pricePerTicket = _session.BasePrice * (1 - discountPercent);
            decimal totalPrice = pricePerTicket * quantity;
            int pointsAdded = 0;

            using (var db = new CinemaDbContext())
            {
                // Проверяем актуальное количество мест в БД
                var sessionInDb = db.Sessions.Find(_session.Id);
                if (sessionInDb == null || sessionInDb.AvailableSeats < quantity)
                {
                    MessageBox.Show(
                        $"Недостаточно свободных мест!\nДоступно: {sessionInDb?.AvailableSeats ?? 0}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Определяем посетителя: если нашли по бонусной карте — используем его,
                // иначе ищем по ФИО или создаём нового разового посетителя
                Visitor visitor;
                if (_foundVisitor != null)
                {
                    visitor = db.Visitors.Find(_foundVisitor.Id)!;
                }
                else
                {
                    // Ищем существующего по ФИО (без бонусной карты)
                    visitor = db.Visitors.FirstOrDefault(v => v.FullName == fullName && v.BonusCardNumber == null)
                              ?? new Visitor { FullName = fullName };

                    if (visitor.Id == 0)
                        db.Visitors.Add(visitor);
                }

                // Создаём билеты
                for (int i = 0; i < quantity; i++)
                {
                    db.Tickets.Add(new Ticket
                    {
                        SessionId = _session.Id,
                        VisitorId = visitor.Id == 0 ? 0 : visitor.Id, // EF подставит после Add
                        Visitor = visitor.Id == 0 ? visitor : null,
                        SeatNumber = i + 1,
                        FinalPrice = pricePerTicket,
                        PurchaseDate = DateTime.Now
                    });
                }

                // Уменьшаем места
                sessionInDb.AvailableSeats -= quantity;

                // Начисляем баллы если есть бонусная карта
                if (_foundVisitor != null)
                {
                    pointsAdded = (int)(totalPrice * 0.10m);
                    visitor.BonusPoints += pointsAdded;
                }

                db.SaveChanges();
            }

            string bonusText = pointsAdded > 0 ? $"\nНачислено баллов: +{pointsAdded}" : "";
            MessageBox.Show(
                $"Спасибо, {fullName.Split(' ')[0]}!\n" +
                $"Куплено билетов: {quantity}\n" +
                $"Сумма: {totalPrice:F0} руб.{bonusText}",
                "Покупка завершена", MessageBoxButton.OK, MessageBoxImage.Information);

            SaveReceipt(fullName, totalPrice, quantity, pointsAdded);

            DialogResult = true;
            Close();
        }

        private void SaveReceipt(string fullName, decimal totalPrice, int quantity, int pointsAdded)
        {
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Чеки");
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            string fileName = $"Чек_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullPath = System.IO.Path.Combine(folderPath, fileName);

            string bonusLine = pointsAdded > 0
                ? $"Начислено баллов: +{pointsAdded}\n"
                : "Бонусная карта: не использована\n";

            string receipt =
                "=== КИНОТЕАТР STARLIGHT ===\n\n" +
                $"Дата покупки:   {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                $"Фильм:          {_session.Film.Title}\n" +
                $"Зал:            {_session.Hall.HallNumber}\n" +
                $"Время сеанса:   {_session.StartDateTime:dd.MM.yyyy HH:mm}\n" +
                $"Кол-во билетов: {quantity}\n" +
                $"Покупатель:     {fullName}\n" +
                bonusLine +
                $"ИТОГО:          {totalPrice:F0} руб.\n\n" +
                "Спасибо за покупку! Приятного просмотра!";

            System.IO.File.WriteAllText(fullPath, receipt);
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void Window_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
    }
}
