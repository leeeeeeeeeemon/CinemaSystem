using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CinemaSystem.Models;
using CinemaSystem.Data;
using System.Windows.Input;

namespace CinemaSystem.Views.Pages
{
    public partial class VisitorsPage : Page
    {
        private List<Visitor> _allVisitors = new List<Visitor>();

        public VisitorsPage()
        {
            InitializeComponent();
            LoadVisitorsFromDatabase();
        }

        private void LoadVisitorsFromDatabase()
        {
            using (var db = new CinemaDbContext())
            {
                _allVisitors = db.Visitors
                    .OrderBy(v => v.FullName)
                    .ToList();

                VisitorsDataGrid.ItemsSource = _allVisitors;
            }
        }

        private void AddVisitorButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditVisitorWindow();
            if (window.ShowDialog() == true)
            {
                LoadVisitorsFromDatabase();
            }
        }

        private void VisitorsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VisitorsDataGrid.SelectedItem is Visitor selectedVisitor)
            {
                var window = new AddEditVisitorWindow(selectedVisitor);
                if (window.ShowDialog() == true)
                {
                    LoadVisitorsFromDatabase();
                }
            }
        }

        private void SearchVisitorBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchVisitorBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(filter))
            {
                VisitorsDataGrid.ItemsSource = _allVisitors;
                return;
            }

            var filtered = _allVisitors.Where(v =>
                v.FullName.ToLower().Contains(filter) ||
                (v.BonusCardNumber?.ToLower() ?? "").Contains(filter)
            ).ToList();

            VisitorsDataGrid.ItemsSource = filtered;
        }
    }
}