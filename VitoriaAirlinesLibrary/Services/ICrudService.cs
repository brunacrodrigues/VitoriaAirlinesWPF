using System.Globalization;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public interface ICrudService<T> where T : class
    {
        Task<Response> GetAllAsync();

        Task<Response> GetByIdAsync(int id);

        Task<Response> CreateAsync(T model);

        Task<Response> UpdateAsync(T model);

        Task<Response> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
