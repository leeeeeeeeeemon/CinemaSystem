using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using CinemaSystem.Models;
using CinemaSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Views.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            LoadTickets();
        }

        private void LoadTickets()
        {
            using (var db = new CinemaDbContext())
            {
                var tickets = db.Tickets
                    .Include(t => t.Session).ThenInclude(s => s.Film)
                    .Include(t => t.Session).ThenInclude(s => s.Hall)
                    .Include(t => t.Visitor)
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToList();

                TicketsDataGrid.ItemsSource = tickets;
            }
        }
    }
}