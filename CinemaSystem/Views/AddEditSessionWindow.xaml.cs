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

            _session = sessionToEdit ?? new Session();
            _isEditMode = sessionToEdit != null;

            Title = _isEditMode ? "Редактирование сеанса" : "Добавление сеанса";

            LoadComboBoxes();

            if (_isEditMode)
                LoadEditData();
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
            cmbFilm.SelectedItem = cmbFilm.Items.Cast<Film>().FirstOrDefault(f => f.Id == _session.FilmId);
            cmbHall.SelectedItem = cmbHall.Items.Cast<Hall>().FirstOrDefault(h => h.Id == _session.HallId);
            dpDate.SelectedDate = _session.StartDateTime.Date;
            tpTime.SelectedTime = _session.StartDateTime;
            txtBasePrice.Text = _session.BasePrice.ToString("0");
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

            DateTime selectedDateTime = dpDate.SelectedDate.Value.Date.Add(tpTime.SelectedTime.Value.TimeOfDay);

            if (selectedDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать сеанс в прошедшую дату и время!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new CinemaDbContext())
            {
                bool isHallOccupied = db.Sessions
                    .Any(s => s.HallId == selectedHall.Id &&
                              s.Id != _session.Id &&
                              s.StartDateTime < selectedDateTime.AddMinutes(180) &&
                              s.StartDateTime.AddMinutes(180) > selectedDateTime);

                if (isHallOccupied)
                {
                    MessageBox.Show("В выбранном зале уже есть сеанс в это время!\nВыберите другое время или другой зал.",
                                    "Конфликт расписания", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _session.FilmId = selectedFilm.Id;
                _session.HallId = selectedHall.Id;
                _session.StartDateTime = selectedDateTime;
                _session.BasePrice = decimal.TryParse(txtBasePrice.Text, out decimal price) ? price : 0m;

                if (!_isEditMode)
                    _session.AvailableSeats = selectedHall.Capacity;

                if (_isEditMode)
                {
                    var existing = db.Sessions.Find(_session.Id);
                    if (existing != null)
                    {
                        existing.FilmId = _session.FilmId;
                        existing.HallId = _session.HallId;
                        existing.StartDateTime = _session.StartDateTime;
                        existing.BasePrice = _session.BasePrice;
                    }
                }
                else
                {
                    db.Sessions.Add(_session);
                }

                db.SaveChanges();
            }

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