using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CinemaSystem.Models;
using CinemaSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace CinemaSystem.Views.Pages
{
    public partial class FilmsPage : Page
    {
        private List<Film> _currentFilms = new List<Film>();
        private bool _isArchiveMode = false;

        public FilmsPage()
        {
            InitializeComponent();

            using (var db = new CinemaDbContext())
            {
                DbInitializer.Initialize(db);
            }

            LoadGenresIntoFilter();
            // Загружаем данные после того, как XAML полностью инициализирован
            Loaded += FilmsPage_Loaded;
        }

        private void FilmsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilmsFromDatabase();
        }

        private void LoadGenresIntoFilter()
        {
            using (var db = new CinemaDbContext())
            {
                var genres = db.Genres.OrderBy(g => g.Name).ToList();

                cmbGenreFilter.Items.Clear();
                cmbGenreFilter.Items.Add(new ComboBoxItem { Content = "Все жанры", IsSelected = true });

                foreach (var genre in genres)
                {
                    cmbGenreFilter.Items.Add(new ComboBoxItem
                    {
                        Content = genre.Name,
                        Tag = genre.Id
                    });
                }
            }
        }

        private void LoadFilmsFromDatabase()
        {
            using (var db = new CinemaDbContext())
            {
                var query = db.Films
                    .Include(f => f.Genre)
                    .Include(f => f.Director)
                    .AsQueryable();

                if (_isArchiveMode)
                {
                    query = query.Where(f => f.IsArchived);
                    ArchiveButtonText.Text = "Показать активные";
                }
                else
                {
                    query = query.Where(f => !f.IsArchived);
                    ArchiveButtonText.Text = "Показать архив";
                }

                _currentFilms = query.ToList();
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            if (FilmsDataGrid == null) return; // защита от NullReference

            var filtered = _currentFilms.AsEnumerable();

            // Фильтр по жанру
            if (cmbGenreFilter.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content.ToString() != "Все жанры")
            {
                if (selectedItem.Tag is int genreId)
                {
                    filtered = filtered.Where(f => f.GenreId == genreId);
                }
            }

            // Текстовый поиск
            string searchText = SearchBox?.Text?.ToLower() ?? "";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(f =>
                    f.Title.ToLower().Contains(searchText) ||
                    (f.Genre?.Name ?? "").ToLower().Contains(searchText) ||
                    (f.Director?.FullName ?? "").ToLower().Contains(searchText)
                );
            }

            FilmsDataGrid.ItemsSource = filtered.ToList();
        }

        // ==================== События ====================

        private void cmbGenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AddFilmButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditFilmWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadFilmsFromDatabase();
            }
        }

        private void DeleteFilmButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilmsDataGrid.SelectedItem is not Film selectedFilm)
            {
                MessageBox.Show("Выберите фильм для удаления!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Перенести в архив фильм:\n«{selectedFilm.Title}»?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new CinemaDbContext())
            {
                var film = db.Films.Find(selectedFilm.Id);
                if (film != null)
                {
                    film.IsArchived = true;
                    db.SaveChanges();
                }
            }
            LoadFilmsFromDatabase();
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            _isArchiveMode = !_isArchiveMode;
            LoadFilmsFromDatabase();
        }

        private void FilmsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilmsDataGrid.SelectedItem is Film selectedFilm)
            {
                var editWindow = new AddEditFilmWindow(selectedFilm);
                if (editWindow.ShowDialog() == true)
                {
                    LoadFilmsFromDatabase();
                }
            }
        }
    }
}