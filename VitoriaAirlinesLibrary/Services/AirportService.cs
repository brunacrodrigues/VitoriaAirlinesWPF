using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class AirportService : ICrudService<Airport>
    {
        ApiService _apiService;
        const string Controller = "airports";

        public AirportService()
        {
            _apiService = new ApiService();
        }

        public Task<Response> GetAllAsync()
        {
            return _apiService.GetAsync<List<Airport>>(Controller);
        }

        public Task<Response> GetByIdAsync(int id)
        {
            return _apiService.GetAsync<Airport>($"{Controller}/{id}");
        }

        public Task<Response> CreateAsync(Airport newAirport)
        {
            return _apiService.PostAsync(Controller, newAirport);
        }

        public Task<Response> UpdateAsync(Airport updatedAirport)
        {
            return _apiService.PutAsync($"{Controller}/{updatedAirport.Id}", updatedAirport);
        }

        public Task<Response> DeleteAsync (int id)
        {
            return _apiService.DeleteAsync($"{Controller}/{id}");
        }

		public async Task<bool> ExistsAsync(int id)
		{
			try
			{
				var response = await _apiService.GetAsync<Airport>($"{Controller}/{id}");


				return response.IsSuccess;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
