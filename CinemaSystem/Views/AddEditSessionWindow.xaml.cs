using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class AddEditSessionWindow : Window
    {
        private readonly Session _session;
        private readonly bool _isEditMode;

        public AddEditSessionWindow(Session? sessionToEdit = null)
        {
            InitializeComponent();

            if (sessionToEdit != null)
            {
                _session = sessionToEdit;
                _isEditMode = true;
                Title = "Редактирование сеанса";
            }
            else
            {
                _session = new Session();
                _isEditMode = false;
                Title = "Добавление сеанса";
            }

            LoadComboBoxes();

            if (_isEditMode)
            {
                LoadEditData();
            }
        }

        private void LoadComboBoxes()
        {
            using (var db = new CinemaDbContext())
            {
                cmbFilm.ItemsSource = db.Films.OrderBy(f => f.Title).ToList();
                cmbHall.ItemsSource = db.Halls.OrderBy(h => h.HallNumber).ToList();
            }
        }

        private void LoadEditData()
        {
            // Явно проверяем, что данные есть
            if (_session.Film != null)
                cmbFilm.SelectedItem = _session.Film;

            if (_session.Hall != null)
                cmbHall.SelectedItem = _session.Hall;

            dpDate.SelectedDate = _session.StartDateTime.Date;
            tpTime.SelectedTime = _session.StartDateTime;   // передаём полный DateTime

            txtBasePrice.Text = _session.BasePrice.ToString("F0");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

            if (dpDate.SelectedDate == null || tpTime.SelectedTime == null)
            {
                MessageBox.Show("Выберите дату и время сеанса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtBasePrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Укажите корректную цену билета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBasePrice.Focus();
                return;
            }

            // Заполняем данные
            _session.FilmId = selectedFilm.Id;
            _session.Film = selectedFilm;
            _session.HallId = selectedHall.Id;
            _session.Hall = selectedHall;
            _session.StartDateTime = dpDate.SelectedDate.Value.Date.Add(tpTime.SelectedTime.Value.TimeOfDay);
            _session.BasePrice = price;

            if (_isEditMode == false)
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

        public Session GetSession() => _session;
    }
}