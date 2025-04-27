using VitoriaAirlinesLibrary.Enums;

namespace VitoriaAirlinesLibrary.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public Flight Flight { get; set; }
        public Seat Seat { get; set; }
        public decimal Price { get; set; }
        public Client? Client { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}
