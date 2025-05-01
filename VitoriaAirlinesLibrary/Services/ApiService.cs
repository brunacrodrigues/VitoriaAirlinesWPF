using System.Drawing;
using System.Net.Http.Json;
using System.Text.Json;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:44331/api/")
            };
        }

        public async Task<Response> GetAsync<T>(string controller)
        {
            try
            {
                var response = await _client.GetAsync(controller);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                var result = JsonSerializer.Deserialize<T>(content);

                return new Response
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch(Exception ex)
            {
                return new Response
                { 
                    IsSuccess = false, 
                    Message = ex.Message 
                };
            }
        }


        public async Task<Response> PostAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(controller, data);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Método PUT genérico
        public async Task<Response> PutAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(controller, data);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Método DELETE genérico
        public async Task<Response> DeleteAsync(string controller)
        {
            try
            {
                var response = await _client.DeleteAsync(controller);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}

