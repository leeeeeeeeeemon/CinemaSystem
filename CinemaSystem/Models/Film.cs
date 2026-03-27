using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CinemaSystem.Models
{
    public class Film
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;

        public int DirectorId { get; set; }
        public Director Director { get; set; } = null!;

        [StringLength(500)]
        public string Announcement { get; set; } = string.Empty;

        public int DurationMin { get; set; }
        public int ReleaseYear { get; set; }

        public bool IsArchived { get; set; } = false;   // для мягкого удаления

        // Many-to-many с актёрами
        public List<FilmActor> FilmActors { get; set; } = new();
        public List<Session> Sessions { get; set; } = new List<Session>();
    }
}