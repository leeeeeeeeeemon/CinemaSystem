using System.Collections.Generic;

namespace CinemaSystem.Models
{
    public class Director
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        public List<Film> Films { get; set; } = new();
    }
}