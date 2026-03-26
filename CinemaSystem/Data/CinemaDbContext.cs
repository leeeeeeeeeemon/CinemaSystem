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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cinema.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-Many связь
            modelBuilder.Entity<FilmActor>()
                .HasKey(fa => new { fa.FilmId, fa.ActorId });

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Film)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(fa => fa.FilmId);

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Actor)
                .WithMany(a => a.FilmActors)
                .HasForeignKey(fa => fa.ActorId);

            // Индексы для быстрого поиска
            modelBuilder.Entity<Film>().HasIndex(f => f.Title);
            modelBuilder.Entity<Film>().HasIndex(f => f.ReleaseYear);
        }
    }
}