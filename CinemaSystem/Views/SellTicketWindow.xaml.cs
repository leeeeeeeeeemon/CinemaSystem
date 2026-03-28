using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class SellTicketWindow : Window
    {
        private readonly Session _session;
        private Visitor _currentVisitor = new Visitor();

        public SellTicketWindow(Session session)
        {
            InitializeComponent();
            _session = session;
            LoadData();
        }

        private void LoadData()
        {
            txtFilmTitle.Text = _session.Film.Title;
            txtSessionInfo.Text = $"Зал {_session.Hall.HallNumber} • {_session.StartDateTime:dd.MM.yyyy HH:mm}";
            txtBasePrice.Text = $"Базовая цена за 1 билет: {_session.BasePrice} руб.";

            using (var db = new CinemaDbContext())
            {
                cmbVisitor.ItemsSource = db.Visitors.OrderBy(v => v.FullName).ToList();
            }

            cmbQuantity.SelectedIndex = 0; // по умолчанию 1 билет
        }

        private void cmbVisitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbVisitor.SelectedItem is Visitor visitor)
            {
                _currentVisitor = visitor;
                UpdatePriceInfo();
            }
        }

        private void UpdatePriceInfo()
        {
            int quantity = int.Parse(((ComboBoxItem)cmbQuantity.SelectedItem).Content.ToString());
            decimal basePrice = _session.BasePrice;
            decimal discountPercent = 0;

            if (_currentVisitor.BonusCardNumber != null)
            {
                if (_currentVisitor.BonusPoints >= 300) discountPercent = 0.15m;
                else if (_currentVisitor.BonusPoints >= 100) discountPercent = 0.10m;
                else discountPercent = 0.05m;
            }

            decimal pricePerTicket = basePrice * (1 - discountPercent);
            decimal totalPrice = pricePerTicket * quantity;

            txtDiscountInfo.Text = $"Скидка: {discountPercent * 100:F0}% (баллов: {_currentVisitor.BonusPoints})";
            txtBonusPoints.Text = $"Текущие баллы посетителя: {_currentVisitor.BonusPoints}";
            txtFinalPrice.Text = $"ИТОГО за {quantity} билет(ов): {totalPrice:F0} руб.";
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVisitor.SelectedItem is not Visitor selectedVisitor)
            {
                MessageBox.Show("Выберите посетителя из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbVisitor.Focus();
                return;
            }

            int quantity = int.Parse(((ComboBoxItem)cmbQuantity.SelectedItem).Content.ToString());
            decimal basePrice = _session.BasePrice;
            decimal discountPercent = 0;

            // Расчёт скидки по количеству баллов
            if (selectedVisitor.BonusCardNumber != null)
            {
                if (selectedVisitor.BonusPoints >= 300)
                    discountPercent = 0.15m;
                else if (selectedVisitor.BonusPoints >= 100)
                    discountPercent = 0.10m;
                else
                    discountPercent = 0.05m;
            }

            decimal pricePerTicket = basePrice * (1 - discountPercent);
            decimal totalPrice = pricePerTicket * quantity;
            int points = 0;
            using (var db = new CinemaDbContext())
            {
                // Создаём билеты
                for (int i = 0; i < quantity; i++)
                {
                    var ticket = new Ticket
                    {
                        SessionId = _session.Id,
                        VisitorId = selectedVisitor.Id,
                        SeatNumber = i + 1,
                        FinalPrice = pricePerTicket,
                        PurchaseDate = DateTime.Now
                    };
                    db.Tickets.Add(ticket);
                }

                // Уменьшаем свободные места в сеансе
                var sessionInDb = db.Sessions.Find(_session.Id);
                if (sessionInDb != null)
                    sessionInDb.AvailableSeats -= quantity;

                // === НАЧИСЛЯЕМ БАЛЛЫ (10% от общей суммы) ===
                int pointsToAdd = (int)(totalPrice * 0.10m);
                selectedVisitor.BonusPoints += pointsToAdd;

                db.SaveChanges();

                points = pointsToAdd;
            }

            MessageBox.Show($"Продано {quantity} билет(ов) на сумму {totalPrice:F0} руб.\n" +
                            $"Начислено баллов: {points}",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            SaveReceiptToFile(selectedVisitor, totalPrice, quantity);
            DialogResult = true;
            Close();
        }

        private void SaveReceiptToFile(Visitor visitor, decimal totalPrice, int quantity)
        {
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Чеки");
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            string fileName = $"Чек_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullPath = System.IO.Path.Combine(folderPath, fileName);

            string receipt =
                "=== КИНОТЕАТР STARLIGHT ===\n\n" +
                $"Дата продажи: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                $"Фильм: {_session.Film.Title}\n" +
                $"Зал: {_session.Hall.HallNumber}\n" +
                $"Время сеанса: {_session.StartDateTime:dd.MM.yyyy HH:mm}\n" +
                $"Количество билетов: {quantity}\n" +
                $"Посетитель: {visitor.FullName}\n" +
                $"Бонусная карта: {visitor.BonusCardNumber ?? "— "}\n" +
                $"Начислено баллов: {(int)(totalPrice * 0.1m)}\n" +
                $"ИТОГО К ОПЛАТЕ: {totalPrice:F0} руб.\n\n" +
                "Спасибо за покупку! Приятного просмотра!";

            System.IO.File.WriteAllText(fullPath, receipt);
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }

        private void CreateNewVisitor_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditVisitorWindow();
            if (window.ShowDialog() == true)
            {
                using (var db = new CinemaDbContext())
                {
                    cmbVisitor.ItemsSource = db.Visitors.OrderBy(v => v.FullName).ToList();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void Window_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
    }
}