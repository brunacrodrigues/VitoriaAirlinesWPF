using VitoriaAirlinesLibrary.Enums;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class FlightSearchService
    {
        private List<Flight> FindOneWayFlights(
            List<Flight> allFlights,
            int originId,
            int destinationId,
            DateTime searchDate,
            SeatType seatType,
            int passengerCount)
        {
            return allFlights.Where(f =>
                f.OriginAirportId == originId &&
                f.DestinationAirportId == destinationId &&
                f.DepartureDateTime.Date == searchDate.Date &&
                HasEnoughAvailableSeats(f, seatType, passengerCount)
            ).ToList();
        }

        private bool HasEnoughAvailableSeats(Flight flight, SeatType type, int requiredSeats)
        {
            return flight.Tickets?.Count(t => t.ClientId == null && t.Seat.Type == type) >= requiredSeats;
        }

        public List<Flight> FilterFlights(
            List<Flight> allFlights,
            int originId,
            int destinationId,
            DateTime departureDate,
            DateTime? returnDate,
            bool isRoundTrip,
            SeatType seatType,
            int passengerCount)
        {
            List<Flight> resultingFlights = new List<Flight>();
            List<Flight> outboundFlightsOriginal = FindOneWayFlights(
                allFlights, originId, destinationId, departureDate, seatType, passengerCount);

            if (!isRoundTrip)
            {
                if (outboundFlightsOriginal.Any())
                {
                    resultingFlights.AddRange(outboundFlightsOriginal);
                }
            }
            else 
            {
                if (returnDate != null)
                {
                    List<Flight> returnFlightsOriginal = FindOneWayFlights(
                        allFlights, destinationId, originId, returnDate.Value, seatType, passengerCount);

                   
                    if (outboundFlightsOriginal.Any() && returnFlightsOriginal.Any())
                    {
                        resultingFlights.AddRange(outboundFlightsOriginal);
                        resultingFlights.AddRange(returnFlightsOriginal);
                    }
                }
            }

            if (isRoundTrip && !resultingFlights.Any())
            {
                if (returnDate.HasValue) 
                {
                    List<Flight> invertedOutboundLeg = FindOneWayFlights(
                        allFlights,
                        destinationId,  
                        originId, 
                        departureDate,
                        seatType,
                        passengerCount);

                    List<Flight> invertedReturnLeg = FindOneWayFlights(
                        allFlights,
                        originId,
                        destinationId,
                        returnDate.Value,
                        seatType,
                        passengerCount);

                    if (invertedOutboundLeg.Any() && invertedReturnLeg.Any())
                    {
                        resultingFlights.AddRange(invertedOutboundLeg);
                        resultingFlights.AddRange(invertedReturnLeg);
                    }
                }
            }

            UpdateDisplayPrices(resultingFlights, seatType);
            return resultingFlights;
        }

        private void UpdateDisplayPrices(List<Flight> flights, SeatType seatType)
        {
            foreach (var flight in flights)
            {
                var ticket = flight.Tickets?.FirstOrDefault(t => t.Seat.Type == seatType);
                flight.DisplayPrice = ticket?.Price;
            }
        }
    }
}