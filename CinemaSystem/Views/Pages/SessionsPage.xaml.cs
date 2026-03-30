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
    public partial class SessionsPage : Page
    {
        private List<Session> _currentSessions = new List<Session>();

        public SessionsPage()
        {
            InitializeComponent();

            // Инициализация базы данных
            using (var db = new CinemaDbContext())
            {
                DbInitializer.Initialize(db);
            }

            LoadSessionsFromDatabase();
        }

        private void LoadSessionsFromDatabase()
        {
            using (var db = new CinemaDbContext())
            {
                _currentSessions = db.Sessions
                    .Include(s => s.Film)
                    .Include(s => s.Hall)
                    .Where(s => !s.IsArchived)
                    .OrderBy(s => s.StartDateTime)
                    .ToList();

                SessionsDataGrid.ItemsSource = _currentSessions;
            }
        }

        // ==================== Добавление сеанса ====================
        private void AddSessionButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditSessionWindow();
            if (window.ShowDialog() == true)
            {
                var newSession = window.GetSession();
                string filmTitle;
                using (var db = new CinemaDbContext())
                {
                    
                    newSession.FilmId = newSession.FilmId;     // уже должен быть установлен в окне
                    newSession.HallId = newSession.HallId;
                    filmTitle = db.Films.FirstOrDefault(x => x.Id == newSession.FilmId).Title;

                    // Сбрасываем Id, чтобы EF Core понял, что это новая запись
                    newSession.Id = 0;

                    db.Sessions.Add(newSession);
                    db.SaveChanges();
                }

                LoadSessionsFromDatabase();

                MessageBox.Show($"Сеанс успешно добавлен!\n" +
                                $"Фильм: {filmTitle}\n" +
                                $"Время: {newSession.StartDateTime:dd.MM.yyyy HH:mm}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ==================== Удаление сеанса ====================
        private void DeleteSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionsDataGrid.SelectedItem is not Session selectedSession)
            {
                MessageBox.Show("Выберите сеанс для удаления!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Перенести в архив сеанс фильма:\n«{selectedSession.Film.Title}»\n" +
                $"{selectedSession.StartDateTime:dd.MM.yyyy HH:mm} ?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new CinemaDbContext())
            {
                var session = db.Sessions.Find(selectedSession.Id);
                if (session != null)
                {
                    session.IsArchived = true;
                    db.SaveChanges();
                }
            }

            LoadSessionsFromDatabase();
            MessageBox.Show("Сеанс перенесён в архив.", "Выполнено");
        }

        private void SessionsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SessionsDataGrid.SelectedItem is Session selectedSession)
            {
                var window = new AddEditSessionWindow(selectedSession);
                if (window.ShowDialog() == true)
                {
                    LoadSessionsFromDatabase();
                }
            }
        }

        // ==================== Поиск ====================
        private void SearchSessionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchSessionBox.Text?.ToLower() ?? "";

            var filtered = _currentSessions.Where(s =>
                s.Film.Title.ToLower().Contains(filter) ||
                s.Hall.HallNumber.ToString().Contains(filter)
            ).ToList();

            SessionsDataGrid.ItemsSource = filtered;
        }

    }
}