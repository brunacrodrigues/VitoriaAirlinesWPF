namespace VitoriaAirlinesLibrary.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public Airplane Airplane { get; set; }
        public Airport Origin { get; set; }
        public Airport Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan Duration { get; set; }
        public List<Ticket> Tickets { get; set; }
    }
}
