using AutoMapper;
using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Services;

public class CoworkingsService : ICoworkingsService
{
    private readonly BookingContext _dbContext;
    private readonly IMapper _mapper;

    public CoworkingsService(BookingContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<List<CoworkingModelDto>> GetAllCoworkingsAsync()
    {
        var coworkings = await _dbContext.Coworking.ToListAsync();
        return _mapper.Map<List<CoworkingModelDto>>(coworkings);
    }
}