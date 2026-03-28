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
        public DbSet<Visitor> Visitors { get; set; }     
        public DbSet<Ticket> Tickets { get; set; }       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cinema.db");
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

            // Связь Ticket -> Session и Ticket -> Visitor
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Session)
                .WithMany()
                .HasForeignKey(t => t.SessionId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Visitor)
                .WithMany()
                .HasForeignKey(t => t.VisitorId);

            modelBuilder.Entity<Session>().HasIndex(s => s.StartDateTime);
            modelBuilder.Entity<Hall>().HasIndex(h => h.HallNumber);
        }
    }
}