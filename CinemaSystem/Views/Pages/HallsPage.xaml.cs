using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views.Pages
{
    public partial class HallsPage : Page
    {
        private List<Hall> _halls = new List<Hall>();

        public HallsPage()
        {
            InitializeComponent();
            LoadHallsFromDatabase();
        }

        private void LoadHallsFromDatabase()
        {
            using (var db = new CinemaDbContext())
            {
                _halls = db.Halls
                    .OrderBy(h => h.HallNumber)
                    .ToList();

                HallsDataGrid.ItemsSource = _halls;
            }
        }

        private void AddHallButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditHallWindow();
            if (window.ShowDialog() == true)
            {
                LoadHallsFromDatabase();
            }
        }

        private void HallsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HallsDataGrid.SelectedItem is Hall selectedHall)
            {
                var window = new AddEditHallWindow(selectedHall);
                if (window.ShowDialog() == true)
                {
                    LoadHallsFromDatabase();
                }
            }
        }
    }
}