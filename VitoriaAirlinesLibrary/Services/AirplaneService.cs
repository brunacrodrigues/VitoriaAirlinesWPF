using System.Text.RegularExpressions;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class AirplaneService : ICrudService<Airplane>
    {
        private readonly ApiService _apiService;
        const string Controller = "airplanes";

        public AirplaneService()
        {
            _apiService = new ApiService();
        }

        public Task<Response> GetAllAsync()
        {
            return _apiService.GetAsync<List<Airplane>>(Controller);
        }

        public Task<Response> GetByIdAsync(int id)
        {
            return _apiService.GetAsync<Airplane>($"{Controller}/{id}");
        }

        public Task<Response> CreateAsync(Airplane model)
        {
            return _apiService.PostAsync(Controller, model);
        }

        public Task<Response> UpdateAsync(Airplane model)
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
				var response = await _apiService.GetAsync<Airplane>($"{Controller}/{id}");


				return response.IsSuccess;
			}
			catch (Exception)
			{
				return false;
			}
		}

        public Task<Response> GetSeatsForAirplaneAsync(int airplaneId)
        {
            return _apiService.GetAsync<List<Seat>>($"{Controller}/{airplaneId}/seats");
        }
    }
}
