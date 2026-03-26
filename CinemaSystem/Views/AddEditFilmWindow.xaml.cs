using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class AddEditFilmWindow : Window
    {
        private readonly Film _film;
        private readonly CinemaDbContext _context;

        public AddEditFilmWindow(Film? filmToEdit = null)
        {
            InitializeComponent();
            _context = new CinemaDbContext();

            // Загружаем жанры один раз
            cmbGenre.ItemsSource = _context.Genres.OrderBy(g => g.Name).ToList();

            if (filmToEdit != null)
            {
                _film = filmToEdit;
                Title = "Редактирование фильма";
                LoadFilmData();
            }
            else
            {
                _film = new Film();
                Title = "Добавление нового фильма";
            }
        }

        private void LoadFilmData()
        {
            txtTitle.Text = _film.Title;
            txtDirector.Text = _film.Director?.FullName ?? "";
            txtReleaseYear.Text = _film.ReleaseYear > 0 ? _film.ReleaseYear.ToString() : "";
            txtDuration.Text = _film.DurationMin > 0 ? _film.DurationMin.ToString() : "";
            txtAnnouncement.Text = _film.Announcement ?? "";

            if (_film.Genre != null)
                cmbGenre.SelectedItem = _film.Genre;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Название фильма обязательно для заполнения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return;
            }

            if (cmbGenre.SelectedItem is not Genre selectedGenre)
            {
                MessageBox.Show("Выберите жанр из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbGenre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDirector.Text))
            {
                MessageBox.Show("Укажите фамилию режиссёра!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDirector.Focus();
                return;
            }

            if (!int.TryParse(txtReleaseYear.Text, out int year) || year < 1900 || year > 2035)
            {
                MessageBox.Show("Год выпуска должен быть числом от 1900 до 2035!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtReleaseYear.Focus();
                return;
            }

            if (!int.TryParse(txtDuration.Text, out int duration) || duration < 10 || duration > 600)
            {
                MessageBox.Show("Длительность должна быть от 10 до 600 минут!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDuration.Focus();
                return;
            }

            // Заполняем объект фильма
            _film.Title = txtTitle.Text.Trim();
            _film.GenreId = selectedGenre.Id;
            _film.Genre = selectedGenre;
            _film.Director = new Director { FullName = txtDirector.Text.Trim() }; // пока упрощённо
            _film.ReleaseYear = year;
            _film.DurationMin = duration;
            _film.Announcement = txtAnnouncement.Text?.Trim() ?? "";

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

        public Film GetFilm() => _film;
    }
}