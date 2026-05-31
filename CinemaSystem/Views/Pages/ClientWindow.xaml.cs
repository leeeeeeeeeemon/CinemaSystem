// Views/ClientWindow.xaml.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Views
{
    public partial class ClientWindow : Window
    {
        private List<Session> _sessions = new List<Session>();

        public ClientWindow()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void LoadSessions()
        {
            using (var db = new CinemaDbContext())
            {
                _sessions = db.Sessions
                    .Include(s => s.Film)
                    .Include(s => s.Hall)
                    .Where(s => !s.IsArchived && s.AvailableSeats > 0)
                    .OrderBy(s => s.StartDateTime)
                    .ToList();
            }

            SessionsGrid.ItemsSource = _sessions;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(filter))
            {
                SessionsGrid.ItemsSource = _sessions;
                return;
            }

            SessionsGrid.ItemsSource = _sessions
                .Where(s => s.Film.Title.ToLower().Contains(filter))
                .ToList();
        }

        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSellTicketWindow();
        }

        private void SessionsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSellTicketWindow();
        }

        private void OpenSellTicketWindow()
        {
            if (SessionsGrid.SelectedItem is not Session selectedSession)
            {
                MessageBox.Show("Выберите сеанс из списка!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sellWindow = new ClientSellTicketWindow(selectedSession);
            if (sellWindow.ShowDialog() == true)
            {
                LoadSessions(); // обновляем количество свободных мест
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}