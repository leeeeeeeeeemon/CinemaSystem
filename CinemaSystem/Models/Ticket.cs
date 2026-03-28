using System;

namespace CinemaSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;

        public int SeatNumber { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime PurchaseDate { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}