using System;
using System.Windows;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class AddEditSessionWindow : Window
    {
        private readonly Session _session = new Session();

        public AddEditSessionWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new CinemaDbContext())
            {
                cmbFilm.ItemsSource = db.Films.OrderBy(f => f.Title).ToList();
                cmbHall.ItemsSource = db.Halls.OrderBy(h => h.HallNumber).ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Логический контроль ввода
            if (cmbFilm.SelectedItem is not Film selectedFilm)
            {
                MessageBox.Show("Выберите фильм!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbFilm.Focus();
                return;
            }

            if (cmbHall.SelectedItem is not Hall selectedHall)
            {
                MessageBox.Show("Выберите кинозал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbHall.Focus();
                return;
            }

            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату сеанса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDate.Focus();
                return;
            }

            if (tpTime.SelectedTime == null)
            {
                MessageBox.Show("Выберите время начала сеанса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                tpTime.Focus();
                return;
            }

            if (!decimal.TryParse(txtBasePrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Укажите корректную базовую цену (больше 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBasePrice.Focus();
                return;
            }

            // Формируем дату и время сеанса
            DateTime datePart = dpDate.SelectedDate.Value.Date;
            DateTime timePart = tpTime.SelectedTime.Value;

            _session.FilmId = selectedFilm.Id;
            _session.Film = selectedFilm;
            _session.HallId = selectedHall.Id;
            _session.Hall = selectedHall;
            _session.StartDateTime = datePart.Add(timePart.TimeOfDay);   
            _session.BasePrice = price;
            _session.AvailableSeats = selectedHall.Capacity;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        public Session GetSession() => _session;
    }
}