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
    public partial class TicketSalesPage : Page
    {
        private List<Session> _availableSessions = new List<Session>();

        public TicketSalesPage()
        {
            InitializeComponent();
            LoadAvailableSessions();
        }

        private void LoadAvailableSessions()
        {
            using (var db = new CinemaDbContext())
            {
                _availableSessions = db.Sessions
                    .Include(s => s.Film)
                    .Include(s => s.Hall)
                    .Where(s => !s.IsArchived && s.AvailableSeats > 0)
                    .OrderBy(s => s.StartDateTime)
                    .ToList();

                AvailableSessionsGrid.ItemsSource = _availableSessions;
            }
        }

        private void SellTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableSessionsGrid.SelectedItem is not Session selectedSession)
            {
                MessageBox.Show("Выберите сеанс для продажи билета!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sellWindow = new SellTicketWindow(selectedSession);
            if (sellWindow.ShowDialog() == true)
            {
                LoadAvailableSessions(); // обновляем список доступных сеансов
            }
        }

        private void AvailableSessionsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SellTicketButton_Click(null, null);
        }
    }
}