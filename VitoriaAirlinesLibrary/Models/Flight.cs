using System.Net.Sockets;
using System.Text.Json.Serialization;
using VitoriaAirlinesLibrary.Enums;

namespace VitoriaAirlinesLibrary.Models
{
	public class Flight
	{
		public int Id { get; set; }
		public string FlightNumber { get; set; }

        [JsonIgnore]
        public Airplane Airplane { get; set; }
        public int AirplaneId { get; set; }

		public int OriginAirportId { get; set; }

		public int DestinationAirportId { get; set; }

		public DateTime DepartureDate { get; set; }
		public TimeSpan DepartureTime { get; set; }
		public TimeSpan Duration { get; set; }

		
		public decimal ExecutivePrice { get; set; }

			
		public decimal EconomicPrice { get; set; }

		[JsonIgnore]
		public List<Ticket> Tickets { get; set; }

		public DateTime DepartureDateTime => DepartureDate.Date + DepartureTime;
		public DateTime ArrivalDateTime => DepartureDateTime + Duration;

        //public int AvailableSeats => Tickets?.Count(t => t.Client == null) ?? 0;

        [JsonIgnore]
        public decimal? ExecutiveTicketPrice
		{
			get
			{
				var ticket = Tickets?.FirstOrDefault(t => t.Seat?.Type == SeatType.Executive);
				return ticket?.Price;
			}
		}

		[JsonIgnore]
		public decimal? EconomicTicketPrice
		{
			get
			{
				var ticket = Tickets?.FirstOrDefault(t => t.Seat?.Type == SeatType.Economic);
				return ticket?.Price;
			}
		}

        [JsonIgnore]
        public decimal? DisplayPrice { get; set; }


        [JsonIgnore]
        public Airport Origin { get; set; }

        [JsonIgnore]
        public Airport Destination { get; set; }

        [JsonIgnore]
        public string FlightRoute
        {
            get
            {
                string originIata = this.Origin?.IATA ?? "N/A";
                string destinationIata = this.Destination?.IATA ?? "N/A";
                return $"{originIata} -> {destinationIata}";
            }
        }

		[JsonIgnore]
        public string FlightDisplayInfo
        {
            get
            {
                string departureFormatted = DepartureDateTime.ToString("dd/MM/yyyy HH:mm");
                return $"{FlightNumber} | {FlightRoute} | {departureFormatted}"; 
            }
        }
    }
}