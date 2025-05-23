using VitoriaAirlinesLibrary.Helpers;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class TicketService
    {
        private readonly ApiService _apiService;
        const string Controller = "tickets";

        public TicketService()
        {
            _apiService = new ApiService();
        }

        public Task<Response> GetByIdAsync(int id)
        {
            return _apiService.GetAsync<Ticket>($"{Controller}/{id}");
        }

        public Task<Response> UpdateAsync(Ticket model)
        {
            return _apiService.PutAsync($"{Controller}/{model.Id}", model);
        }


        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<Ticket>($"{Controller}/{id}");


                return response.IsSuccess;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<Response> GetAvailableTicketByFlightAndSeatAsync(int flightId, int seatId)
        {
            return _apiService.GetAsync<Ticket>($"{Controller}/availableByFlightAndSeat?flightId={flightId}&seatId={seatId}");
        }

        public Task<Response> CancelTicketAsync(int id)
        {
            return _apiService.PutAsync<object>($"{Controller}/{id}/cancel", null);
        }
    }
}
