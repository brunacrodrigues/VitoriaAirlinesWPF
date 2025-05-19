using System.Text.Json.Serialization;

namespace VitoriaAirlinesLibrary.Models
{
    public class Airplane
    {
        public int Id { get; set; }

        public string Model { get; set; }

        public bool IsActive { get; set; }

        public int ExecutiveSeats { get; set; }

        public int EconomicSeats { get; set; }

        [JsonIgnore]
        public List<Seat> Seats { get; set; }

        [JsonIgnore]
        public string StatusText => IsActive ? "Active" : "Inactive";

        [JsonIgnore]
        public int TotalCapacity => ExecutiveSeats + EconomicSeats;
	}
}
