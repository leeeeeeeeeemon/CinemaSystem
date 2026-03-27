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
            using (var db = new CinemaDbContext())
            {
                DbInitializer.Initialize(db);   // инициализация справочников и тестовых данных
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

        private void AddSessionButton_Click(object sender, RoutedEventArgs e)
        {
            var addSessionWindow = new AddEditSessionWindow();

            if (addSessionWindow.ShowDialog() == true)
            {
                var newSession = addSessionWindow.GetSession();

                using (var db = new CinemaDbContext())
                {
                    var film = db.Films.Find(newSession.FilmId);
                    var hall = db.Halls.Find(newSession.HallId);

                    if (film != null) newSession.Film = film;
                    if (hall != null) newSession.Hall = hall;

                    db.Sessions.Add(newSession);
                    db.SaveChanges();
                }

                LoadSessionsFromDatabase();

                MessageBox.Show($"Сеанс успешно добавлен!\nФильм: {newSession.Film.Title}\n" +
                                $"Время: {newSession.StartDateTime:dd.MM.yyyy HH:mm}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionsDataGrid.SelectedItem is not Session selectedSession)
            {
                MessageBox.Show("Выберите сеанс для удаления!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Перенести в архив сеанс фильма «{selectedSession.Film.Title}»\n" +
                $"{selectedSession.StartDateTime:dd.MM.yyyy HH:mm}?",
                "Подтверждение",
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
        }

        private void SearchSessionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchSessionBox.Text?.ToLower() ?? "";

            var filtered = _currentSessions.Where(s =>
                s.Film.Title.ToLower().Contains(filter) ||
                s.Hall.HallNumber.ToString().Contains(filter)
            ).ToList();

            SessionsDataGrid.ItemsSource = filtered;
        }

        private void SessionsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SessionsDataGrid.SelectedItem is Session selectedSession)
            {
                MessageBox.Show("Редактирование сеанса будет реализовано позже.", "Информация");
                // TODO: позже вызовем AddEditSessionWindow в режиме редактирования
            }
        }
    }
}