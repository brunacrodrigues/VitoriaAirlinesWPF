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


		[JsonIgnore]
		public Airport Origin { get; set; } 
		public int OriginAirportId { get; set; }


		[JsonIgnore]
		public Airport Destination { get; set; } 
		public int DestinationAirportId { get; set; }

		public DateTime DepartureDate { get; set; }
		public TimeSpan DepartureTime { get; set; }
		public TimeSpan Duration { get; set; }

		public decimal ExecutivePrice { get; set; }

		public decimal EconomicPrice { get; set; }
		public List<Ticket> Tickets { get; set; }

		public DateTime DepartureDateTime => DepartureDate.Date + DepartureTime;
		public DateTime ArrivalDateTime => DepartureDateTime + Duration;

		//public int AvailableSeats => Tickets?.Count(t => t.Client == null) ?? 0;

		
		public decimal? ExecutiveTicketPrice
		{
			get
			{
				var ticket = Tickets?.FirstOrDefault(t => t.Seat?.Type == SeatType.Executive);
				return ticket?.Price;
			}
		}

		public decimal? EconomicTicketPrice
		{
			get
			{
				var ticket = Tickets?.FirstOrDefault(t => t.Seat?.Type == SeatType.Economic);
				return ticket?.Price;
			}
		}
	}
}