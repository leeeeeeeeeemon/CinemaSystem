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
        private List<Film> _allFilms = new List<Film>();

        public FilmsPage()
        {
            InitializeComponent();

            using (var context = new CinemaDbContext())
            {
                DbInitializer.Initialize(context);
            }

            LoadFilmsFromDatabase();
        }

        private void LoadFilmsFromDatabase()
        {
            _allFilms = _context.Films
                .Include(f => f.Genre)
                .Include(f => f.Director)
                .Where(f => !f.IsArchived)
                .ToList();

            FilmsDataGrid.ItemsSource = _allFilms;
        }

        private void AddFilmButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditFilmWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newFilm = addWindow.GetFilm();

                using (var db = new CinemaDbContext())   // новый контекст для сохранения
                {
                    // Важно! Прикрепляем существующий жанр, чтобы не создавать дубликат
                    var existingGenre = db.Genres.Find(newFilm.GenreId);
                    if (existingGenre != null)
                    {
                        newFilm.Genre = existingGenre;
                    }

                    db.Films.Add(newFilm);
                    db.SaveChanges();
                }

                LoadFilmsFromDatabase();

                MessageBox.Show($"Фильм «{newFilm.Title}» успешно добавлен в базу данных!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(filter))
            {
                FilmsDataGrid.ItemsSource = _allFilms;
                return;
            }

            var filtered = _allFilms.Where(f =>
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