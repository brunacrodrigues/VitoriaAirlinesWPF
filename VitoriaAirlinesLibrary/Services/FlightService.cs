﻿using VitoriaAirlinesLibrary.Helpers;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class FlightService : ICrudService<Flight>
    {
        private readonly ApiService _apiService;
        private readonly AirplaneService _airplaneService;
        private readonly AirportService _airportService;

        const string Controller = "flights";

        public FlightService()
        {
            _apiService = new ApiService();
            _airplaneService = new AirplaneService();
            _airportService = new AirportService();
        }

        public async Task<Response> GetAllAsync()
        {
            try
            {
                var flightsResponse = await _apiService.GetAsync<List<Flight>>(Controller);

                if (!flightsResponse.IsSuccess || flightsResponse.Result is not List<Flight> flights)
                    return flightsResponse;

                var loadTasks = flights.Select(async flight =>
                {
                    await LoadAirplaneAsync(flight);
                    await LoadOriginAsync(flight);
                    await LoadDestinationAsync(flight);
                }).ToList();

                await Task.WhenAll(loadTasks);

                return new Response
                {
                    IsSuccess = true,
                    Result = flights
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = $"Error loading flights: {ex.Message}"
                };
            }
        }

        private async Task LoadAirplaneAsync(Flight flight)
        {
            if (flight.AirplaneId <= 0) return;

            var response = await _airplaneService.GetByIdAsync(flight.AirplaneId);
            if (response.IsSuccess && response.Result is Airplane airplane)
            {
                flight.Airplane = airplane;
            }

        }

        private async Task LoadOriginAsync(Flight flight)
        {
            if (flight.OriginAirportId <= 0) return;

            var response = await _airportService.GetByIdAsync(flight.OriginAirportId);
            flight.Origin = response.IsSuccess && response.Result is Airport origin
                ? origin
                : new Airport { IATA = "N/A" };
        }

        private async Task LoadDestinationAsync(Flight flight)
        {
            if (flight.DestinationAirportId <= 0) return;

            var response = await _airportService.GetByIdAsync(flight.DestinationAirportId);
            flight.Destination = response.IsSuccess && response.Result is Airport destination
                ? destination
                : new Airport { IATA = "N/A" };
        }



        public Task<Response> GetByIdAsync(int id)
        {

            return _apiService.GetAsync<Flight>($"{Controller}/{id}");
        }

        public Task<Response> CreateAsync(Flight model)
        {
            return _apiService.PostAsync(Controller, model);
        }

        public Task<Response> UpdateAsync(Flight model)
        {
            return _apiService.PutAsync($"{Controller}/{model.Id}", model);
        }

        public Task<Response> DeleteAsync(int id)
        {
            return _apiService.DeleteAsync($"{Controller}/{id}");
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<Flight>($"{Controller}/{id}");


                return response.IsSuccess;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<Response> GetTicketsForFlightAsync(int flightId)
        {
            return _apiService.GetAsync<List<Ticket>>($"{Controller}/{flightId}/tickets");
        }

        public Task<Response> GetClientsForFlightAsync(int flightId)
        {
            return _apiService.GetAsync<List<Client>>($"{Controller}/{flightId}/clients");
        }

        public async Task<Response> CheckIfClientHasTicketFlightAsync(int flightId, int clientId)
        {
            try
            {
                var ticketsResponse = await GetTicketsForFlightAsync(flightId);

                if (!ticketsResponse.IsSuccess || ticketsResponse.Result is not List<Ticket> tickets)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = ticketsResponse.Message ?? "Failed to retrieve tickets."
                    };
                }

                bool hasTicket = tickets.Any(t => t.ClientId == clientId);

                return new Response
                {
                    IsSuccess = true,
                    Result = hasTicket,
                    Message = "Client booking status checked."
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Result = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public Task <Response> GetFlightsAndTicketsAsync()
        {
            return _apiService.GetAsync<List<Flight>>($"{Controller}/tickets");
        }

    }
}

