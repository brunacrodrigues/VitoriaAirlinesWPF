using System.Text.Json.Serialization;
using VitoriaAirlinesLibrary.Enums;

namespace VitoriaAirlinesLibrary.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public string Letter { get; set; }
        public SeatType Type { get; set; }
        public bool IsAvailable { get; set; }

        public int AirplaneId { get; set; }

        [JsonIgnore]
        public string Name => $"{Row}{Letter}";

    }
}
