using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using VitoriaAirlinesLibrary.Models;


namespace VitoriaAirlinesLibrary.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly JsonSerializerOptions _jsonSerializerOptionsDeserialize;

        public ApiService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:44331/api/")
            };

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNameCaseInsensitive = true
            };

            _jsonSerializerOptionsDeserialize = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

			_jsonSerializerOptionsDeserialize.Converters.Add(new JsonStringEnumConverter());

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

                var result = JsonSerializer.Deserialize<T>(content, _jsonSerializerOptionsDeserialize);

                return new Response
                {
                    IsSuccess = true,
                    Result = result
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

        public async Task<Response> PostAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(controller, data, _jsonSerializerOptions);

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

        public async Task<Response> PutAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(controller, data, _jsonSerializerOptions);

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


