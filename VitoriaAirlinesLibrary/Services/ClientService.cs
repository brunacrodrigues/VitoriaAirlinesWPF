using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class ClientService
    {
        ApiService _apiService;
        const string Controller = "clients";

        public ClientService()
        {
            _apiService = new ApiService();
        }

        public Task<Response> GetAllAsync()
        {
            return _apiService.GetAsync<List<Client>>(Controller);
        }

        public Task<Response> GetByIdAsync(int id)
        {
            return _apiService.GetAsync<Client>($"{Controller}/{id}");
        }

        public Task<Response> CreateAsync(Client newClient)
        {
            return _apiService.PostAsync(Controller, newClient);
        }

        public Task<Response> UpdateAsync(Client updatedClient)
        {
            return _apiService.PutAsync($"{Controller}/{updatedClient.Id}", updatedClient);
        }

        public Task<Response> DeleteAsync(int id)
        {
            return _apiService.DeleteAsync($"{Controller}/{id}");
        }
    }
    
}
