namespace CinemaSystem.Models
{
    public class Visitor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? BonusCardNumber { get; set; }
        public int BonusPoints { get; set; } = 0;
    }
}