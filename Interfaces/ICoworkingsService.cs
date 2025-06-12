using booking_api.Models;

namespace booking_api.Interfaces;

public interface ICoworkingsService
{
    Task<List<CoworkingModelDto>> GetAllCoworkingsAsync();
}