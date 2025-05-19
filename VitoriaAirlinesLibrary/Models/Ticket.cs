using System.Text.Json.Serialization;
using VitoriaAirlinesLibrary.Enums;

namespace VitoriaAirlinesLibrary.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [JsonIgnore]
        public Flight Flight { get; set; }

        public int FlightId { get; set; }

        
        public Seat Seat { get; set; }

        public int SeatId { get; set; } 

        public decimal Price { get; set; }

        [JsonIgnore]
        public Client? Client { get; set; }

        public int? ClientId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod? PaymentMethod { get; set; }
    }
}
