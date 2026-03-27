using System;

namespace CinemaSystem.Models
{
    public class Session
    {
        public int Id { get; set; }

        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;

        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public DateTime StartDateTime { get; set; }
        public decimal BasePrice { get; set; }

        public int AvailableSeats { get; set; }   // будет рассчитываться позже

        public bool IsArchived { get; set; } = false;
    }
}