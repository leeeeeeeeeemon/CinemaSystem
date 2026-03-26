using System.Collections.Generic;

namespace CinemaSystem.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Film> Films { get; set; } = new();
    }
}