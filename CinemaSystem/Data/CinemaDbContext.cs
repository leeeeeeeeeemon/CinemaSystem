using Microsoft.EntityFrameworkCore;
using CinemaSystem.Models;

namespace CinemaSystem.Data
{
    public class CinemaDbContext : DbContext
    {
        public DbSet<Film> Films { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<FilmActor> FilmActors { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Session> Sessions { get; set; }

        private static string DbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CinemaSystem",
            "cinema.db");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var directory = System.IO.Path.GetDirectoryName(DbPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilmActor>()
                .HasKey(fa => new { fa.FilmId, fa.ActorId });

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Film)
                .WithMany(f => f.Sessions)
                .HasForeignKey(s => s.FilmId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HallId);

            modelBuilder.Entity<Session>().HasIndex(s => s.StartDateTime);
            modelBuilder.Entity<Hall>().HasIndex(h => h.HallNumber);

            // Добавляем индексы для предотвращения дублирования
            modelBuilder.Entity<Genre>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Director>().HasIndex(d => d.FullName).IsUnique();
        }
    }
}