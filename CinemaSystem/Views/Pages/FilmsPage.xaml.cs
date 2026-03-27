using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Views.Pages
{
    public partial class FilmsPage : Page
    {
        private readonly CinemaDbContext _context = new CinemaDbContext();
        private List<Film> _currentFilms = new List<Film>();
        private bool _isArchiveMode = false;

        public FilmsPage()
        {
            InitializeComponent();
            using (var db = new CinemaDbContext())
            {
                DbInitializer.Initialize(db);
            }
            LoadFilmsFromDatabase();
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
                FilmsDataGrid.ItemsSource = _currentFilms;
            }
        }

        private void AddFilmButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditFilmWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newFilm = addWindow.GetFilm();

                using (var db = new CinemaDbContext())
                {
                    var existingGenre = db.Genres.Find(newFilm.GenreId);
                    if (existingGenre != null)
                        newFilm.Genre = existingGenre;

                    db.Films.Add(newFilm);
                    db.SaveChanges();
                }

                LoadFilmsFromDatabase();

                MessageBox.Show($"Фильм «{newFilm.Title}» успешно добавлен!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var result = MessageBox.Show(
                $"Перенести в архив фильм:\n\n«{selectedFilm.Title}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

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
            MessageBox.Show($"Фильм «{selectedFilm.Title}» перенесён в архив.", "Выполнено");
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            _isArchiveMode = !_isArchiveMode;
            LoadFilmsFromDatabase();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text?.ToLower() ?? "";

            var filtered = _currentFilms.Where(f =>
                f.Title.ToLower().Contains(filter) ||
                (f.Genre?.Name ?? "").ToLower().Contains(filter) ||
                (f.Director?.FullName ?? "").ToLower().Contains(filter)
            ).ToList();

            FilmsDataGrid.ItemsSource = filtered;
        }

        private void FilmsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilmsDataGrid.SelectedItem is Film selectedFilm)
            {
                var editWindow = new AddEditFilmWindow(selectedFilm);
                if (editWindow.ShowDialog() == true)
                {
                    using (var db = new CinemaDbContext())
                    {
                        var filmToUpdate = db.Films.Find(selectedFilm.Id);
                        if (filmToUpdate != null)
                        {
                            filmToUpdate.Title = selectedFilm.Title;
                            filmToUpdate.GenreId = selectedFilm.GenreId;
                            filmToUpdate.Director = selectedFilm.Director;
                            filmToUpdate.ReleaseYear = selectedFilm.ReleaseYear;
                            filmToUpdate.DurationMin = selectedFilm.DurationMin;
                            filmToUpdate.Announcement = selectedFilm.Announcement;
                        }
                        db.SaveChanges();
                    }
                    LoadFilmsFromDatabase();
                }
            }
        }
    }
}   