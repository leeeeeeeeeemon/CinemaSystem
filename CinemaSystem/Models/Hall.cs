namespace CinemaSystem.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public int HallNumber { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<Session> Sessions { get; set; } = new List<Session>();
    }
}