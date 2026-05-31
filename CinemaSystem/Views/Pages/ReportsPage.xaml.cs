using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

            // Устанавливаем даты по умолчанию: последние 30 дней
            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-30);
            DateToPicker.SelectedDate = DateTime.Now;

            LoadFilmsFilter();
            LoadAllReports();
        }

        private void LoadFilmsFilter()
        {
            using (var db = new CinemaDbContext())
            {
                var films = db.Films.OrderBy(f => f.Title).ToList();
                films.Insert(0, new Film { Id = 0, Title = "Все фильмы" });
                FilmFilterComboBox.ItemsSource = films;
                FilmFilterComboBox.SelectedIndex = 0;
            }
        }

        private void LoadAllReports()
        {
            LoadTicketsReport();
            LoadStatisticsCards();
            LoadFilmsStatistics();
            LoadDailyStatistics();
        }

        private void LoadTicketsReport()
        {
            using (var db = new CinemaDbContext())
            {
                var query = db.Tickets
                    .Include(t => t.Session).ThenInclude(s => s.Film)
                    .Include(t => t.Session).ThenInclude(s => s.Hall)
                    .Include(t => t.Visitor)
                    .AsQueryable();

                // Применяем фильтры
                query = ApplyFilters(query);

                var tickets = query
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToList();

                TicketsDataGrid.ItemsSource = tickets;
            }
        }

        private void LoadStatisticsCards()
        {
            using (var db = new CinemaDbContext())
            {
                var query = db.Tickets.AsQueryable();
                query = ApplyFilters(query);

                // Получаем данные сначала в память, потом считаем суммы
                var tickets = query.ToList();

                var totalRevenue = tickets.Sum(t => (double)t.FinalPrice);
                var totalTickets = tickets.Count;
                var averageTicket = totalTickets > 0 ? totalRevenue / totalTickets : 0;

                TotalRevenueText.Text = $"{totalRevenue:F2} руб.";
                TotalTicketsText.Text = totalTickets.ToString();
                AverageTicketText.Text = $"{averageTicket:F2} руб.";
            }
        }

        private void LoadFilmsStatistics()
        {
            using (var db = new CinemaDbContext())
            {
                var query = db.Tickets
                    .Include(t => t.Session)
                    .ThenInclude(s => s.Film)
                    .AsQueryable();

                query = ApplyFilters(query);

                // Получаем данные в память и группируем на клиенте
                var tickets = query.ToList();

                var filmsStats = tickets
                    .GroupBy(t => t.Session.Film.Title)
                    .Select(g => new FilmStatistic
                    {
                        FilmTitle = g.Key,
                        TicketsSold = g.Count(),
                        Revenue = g.Sum(t => (double)t.FinalPrice)
                    })
                    .OrderByDescending(f => f.Revenue)
                    .ToList();

                FilmsStatsDataGrid.ItemsSource = filmsStats;
            }
        }

        private void LoadDailyStatistics()
        {
            using (var db = new CinemaDbContext())
            {
                var query = db.Tickets.AsQueryable();
                query = ApplyFilters(query);

                // Получаем данные в память и группируем на клиенте
                var tickets = query.ToList();

                var dailyStats = tickets
                    .GroupBy(t => t.PurchaseDate.Date)
                    .Select(g => new DailyStatistic
                    {
                        Date = g.Key,
                        TicketsCount = g.Count(),
                        Revenue = g.Sum(t => (double)t.FinalPrice)
                    })
                    .OrderByDescending(d => d.Date)
                    .ToList();

                DailyStatsDataGrid.ItemsSource = dailyStats;
            }
        }

        private IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query)
        {
            // Фильтр по дате от
            if (DateFromPicker.SelectedDate.HasValue)
            {
                var dateFrom = DateFromPicker.SelectedDate.Value.Date;
                query = query.Where(t => t.PurchaseDate >= dateFrom);
            }

            // Фильтр по дате до
            if (DateToPicker.SelectedDate.HasValue)
            {
                var dateTo = DateToPicker.SelectedDate.Value.Date.AddDays(1);
                query = query.Where(t => t.PurchaseDate < dateTo);
            }

            // Фильтр по фильму
            if (FilmFilterComboBox.SelectedItem is Film selectedFilm && selectedFilm.Id > 0)
            {
                query = query.Where(t => t.Session.FilmId == selectedFilm.Id);
            }

            return query;
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadAllReports();
        }

        private void FilmFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadAllReports();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DateFromPicker.SelectedDate = DateTime.Now.AddDays(-30);
            DateToPicker.SelectedDate = DateTime.Now;
            FilmFilterComboBox.SelectedIndex = 0;

            // LoadAllReports() вызовется через события изменения
        }
    }

    // Классы для статистики
    public class FilmStatistic
    {
        public string FilmTitle { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public double Revenue { get; set; }
    }

    public class DailyStatistic
    {
        public DateTime Date { get; set; }
        public int TicketsCount { get; set; }
        public double Revenue { get; set; }
    }
}