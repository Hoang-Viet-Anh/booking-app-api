using AutoMapper;
using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Services;

public class BookingsService : IBookingsService
{
    private readonly BookingContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingsService> _logger;

    private const int START_HOUR = 6;
    private const int END_HOUR = 18;
    private const int MIN_SESSION = 1;

    public BookingsService(BookingContext context, IMapper mapper, ILogger<BookingsService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<BookingModelDto>> GetAllBookingAsync()
    {
        var booking = await _context.Booking.ToListAsync();
        return _mapper.Map<List<BookingModelDto>>(booking);
    }

    public async Task<BookingModelDto?> GetBookingByIdAsync(Guid id)
    {
        var booking = await _context.Booking.FirstOrDefaultAsync(x => x.Id == id);
        return _mapper.Map<BookingModelDto>(booking);
    }

    public async Task<List<BookingModelDto>?> AddBookingAsync(BookingModelDto bookingDto)
    {
        var workspace = await _context.Workspace.Include(w => w.Availability)
            .FirstOrDefaultAsync(w => w.Id == bookingDto.WorkspaceId);

        if (workspace == null) return null;
        _logger.LogInformation("Adding booking {@bookingDto}", bookingDto);

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, workspace);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto, workspace);

        return bookingList;
    }

    public async Task<List<BookingModelDto>?> EditBookingAsync(Guid id, BookingModelDto bookingDto)
    {
        var existingBooking = await _context.Booking.FirstOrDefaultAsync(x => x.Id == id);
        if (existingBooking == null) return null;
        var workspace = await _context.Workspace.FirstOrDefaultAsync(w => w.Id == bookingDto.WorkspaceId);
        if (workspace == null) return null;

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, workspace);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto, workspace);
        _context.Booking.Remove(existingBooking);
        await _context.SaveChangesAsync();

        return bookingList;
    }

    public async Task DeleteBookingByIdAsync(Guid id)
    {
        var booking = await _context.Booking.FirstOrDefaultAsync(x => x.Id == id);
        if (booking == null) return;
        _context.Booking.Remove(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<DateTime>> GetAllBookingDatesAsync(Guid workspaceId, List<int> capacityList)
    {
        var workspace = await _context.Workspace.Include(w => w.Availability)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null || capacityList.Count == 0) return [];

        var fullyBookedDates = new HashSet<DateTime>();

        foreach (var size in capacityList)
        {
            var maxRooms = workspace.Availability.Rooms.Find(r => r.Capacity == size)?.RoomsAmount ?? 0;
            if (maxRooms == 0) continue;

            var bookings = await _context.Booking.Where(b => b.WorkspaceId == workspaceId && b.RoomSizes.Contains(size))
                .ToListAsync();
            
            var dailyHours = new Dictionary<DateTime, double>();

            foreach (var booking in bookings)
            {
                var start = booking.DateSlot.StartDate.ToUniversalTime();
                var end = booking.DateSlot.EndDate.ToUniversalTime();
                
                for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
                {
                    var workStart = day.AddHours(START_HOUR);
                    var workEnd = day.AddHours(END_HOUR);

                    var overlapStart = (start > workStart) ? start : workStart;
                    var overlapEnd = (end < workEnd) ? end : workEnd;

                    if (overlapStart >= overlapEnd) continue;

                    var hours = (overlapEnd - overlapStart).TotalHours;

                    if (!dailyHours.TryAdd(day, hours))
                    {
                        dailyHours[day] += hours;
                    }
                }
            }
            
            foreach (var (day, totalHours) in dailyHours)
            {
                if (totalHours >= 12 * maxRooms)
                    fullyBookedDates.Add(day);
            }
        }

        return fullyBookedDates.OrderBy(d => d).ToList();
    }

    public async Task<AvailableTimesDto?> GetAvailableTimeAsync(DateSlot dateSlot,
        Guid workspaceId,
        List<int> capacityList)
    {
        var bookings = await _context.Booking
            .Where(b => b.WorkspaceId == workspaceId && b.RoomSizes.Any(r => capacityList.Contains(r)))
            .ToListAsync();
        var workspace = await _context.Workspace.FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace == null) return null;
        var timeSlots = new AvailableTimesDto
        {
            StartTimes = GetAvailableTimeSlotsForDay(dateSlot.StartDate, bookings, workspace, capacityList),
            EndTimes = GetAvailableTimeSlotsForDay(dateSlot.EndDate, bookings, workspace, capacityList)
        };
        return timeSlots;
    }

    private List<DateTime> GetAvailableTimeSlotsForDay(
        DateTime date,
        List<BookingModel> bookings,
        WorkspaceModel workspace,
        List<int> capacityList)
    {
        var dayStart = date.Date.AddHours(START_HOUR);
        var dayEnd = date.Date.AddHours(END_HOUR);

        var availableTimeSlots = new HashSet<DateTime>();

        var maxRooms = capacityList.Sum(size =>
            workspace.Availability.Rooms.FirstOrDefault(r => r.Capacity == size)?.RoomsAmount ?? 0);

        for (var slot = dayStart; slot < dayEnd; slot = slot.AddHours(MIN_SESSION))
        {
            var slotEnd = slot.AddHours(MIN_SESSION);
            var occupiedRooms = 0;

            foreach (var booking in bookings)
            {
                if (booking.DateSlot.EndDate <= slot || booking.DateSlot.StartDate >= slotEnd)
                    continue;

                occupiedRooms += booking.RoomSizes.Count(rs => capacityList.Contains(rs));
            }

            if (occupiedRooms >= maxRooms)
            {
                continue;
            }

            availableTimeSlots.Add(slot);
            availableTimeSlots.Add(slotEnd);
        }

        return availableTimeSlots.ToList();
    }


    private async Task<List<BookingModelDto>?> AddMultipleBooking(BookingModelDto bookingDto, WorkspaceModel workspace)
    {
        var bookingList = new List<BookingModelDto>();

        foreach (var size in bookingDto.RoomSizes)
        {
            var booking = _mapper.Map<BookingModel>(bookingDto);
            booking.Id = Guid.NewGuid();
            booking.workspace = workspace;
            booking.RoomSizes = [size];
            var entry = await _context.Booking.AddAsync(booking);
            bookingList.Add(_mapper.Map<BookingModelDto>(entry.Entity));
        }

        await _context.SaveChangesAsync();

        return bookingList;
    }

    private async Task<Boolean> CheckIfBookingAvailable(BookingModelDto bookingDto, WorkspaceModel workspace)
    {
        foreach (var size in bookingDto.RoomSizes)
        {
            var maxRooms = workspace.Availability.Rooms.Find(r => r.Capacity == size)?.RoomsAmount ?? 0;

            var occupiedAmount = await _context.Booking.CountAsync(b =>
                b.Id != bookingDto.Id &&
                b.WorkspaceId == workspace.Id &&
                b.RoomSizes.Contains(size) &&
                b.DateSlot.StartDate < bookingDto.DateSlot.EndDate &&
                b.DateSlot.EndDate > bookingDto.DateSlot.StartDate);

            if (occupiedAmount >= maxRooms)
            {
                return false;
            }
        }

        return true;
    }
}