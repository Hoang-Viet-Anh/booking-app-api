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

    private const int StartHourUtc = 6;
    private const int EndHourUtc = 18;
    private const int MinSessionLength = 1;

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
        var workspace = await _context.Workspace.FirstOrDefaultAsync(w => w.Id == bookingDto.WorkspaceId);
        var coworking = await _context.Coworking.Include(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(cow => cow.Id == bookingDto.CoworkingId);

        if (workspace == null || coworking == null) return null;

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, coworking);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto, workspace, coworking);

        return bookingList;
    }

    public async Task<List<BookingModelDto>?> EditBookingAsync(Guid id, BookingModelDto bookingDto)
    {
        var existingBooking = await _context.Booking
            .Include(b => b.Coworking)
            .ThenInclude(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (existingBooking == null) return null;

        var workspace = await _context.Workspace.FirstOrDefaultAsync(w => w.Id == bookingDto.WorkspaceId);
        if (workspace == null) return null;

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, existingBooking.Coworking);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto, workspace, existingBooking.Coworking);
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

    public async Task<List<DateTime>> GetAllBookingDatesAsync(Guid coworkingId, Guid workspaceId,
        List<int> capacityList)
    {
        var coworking = await _context.Coworking.Include(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(coworking => coworking.Id == coworkingId);
        if (coworking == null || capacityList.Count == 0) return [];

        var fullyBookedDates = new HashSet<DateTime>();

        foreach (var size in capacityList)
        {
            var maxRooms = coworking.WorkspacesCapacity.Find(wc => wc.WorkspaceId == workspaceId)?
                .Availability.Find(r => r.Capacity == size)?.Amounts ?? 0;
            if (maxRooms == 0) continue;

            var bookings = await _context.Booking.Where(b =>
                    b.WorkspaceId == workspaceId &&
                    b.CoworkingId == coworkingId &&
                    b.RoomSizes.Contains(size))
                .ToListAsync();

            var dailyHours = new Dictionary<DateTime, double>();

            foreach (var booking in bookings)
            {
                var start = booking.DateSlot.StartDate.ToUniversalTime();
                var end = booking.DateSlot.EndDate.ToUniversalTime();

                for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
                {
                    var workStart = day.AddHours(StartHourUtc);
                    var workEnd = day.AddHours(EndHourUtc);

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
        Guid coworkingId,
        List<int> capacityList)
    {
        var bookings = await _context.Booking
            .Where(b =>
                b.WorkspaceId == workspaceId &&
                b.CoworkingId == coworkingId &&
                b.RoomSizes.Any(r => capacityList.Contains(r)))
            .ToListAsync();

        var workspace = await _context.Workspace.FirstOrDefaultAsync(w => w.Id == workspaceId);
        var coworking = await _context.Coworking.Include(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(coworking => coworking.Id == coworkingId);

        if (workspace == null || coworking == null) return null;

        var timeSlots = new AvailableTimesDto
        {
            StartTimes =
                GetAvailableTimeSlotsForDay(dateSlot.StartDate, coworking, workspaceId, bookings, capacityList),
            EndTimes = GetAvailableTimeSlotsForDay(dateSlot.EndDate, coworking, workspaceId, bookings, capacityList)
        };
        return timeSlots;
    }

    private List<DateTime> GetAvailableTimeSlotsForDay(
        DateTime date,
        CoworkingModel coworking,
        Guid workspaceId,
        List<BookingModel> bookings,
        List<int> capacityList)
    {
        var isToday = DateTime.UtcNow.Date == date.ToUniversalTime().Date;
        var availableStartDate = isToday ? DateTime.UtcNow.Hour : StartHourUtc;

        var dayStart = date.Date.AddHours(availableStartDate + 1);
        var dayEnd = date.Date.AddHours(EndHourUtc);

        var availableTimeSlots = new HashSet<DateTime>();

        if (availableStartDate >= EndHourUtc - 1)
        {
            return availableTimeSlots.ToList();
        }

        var maxRooms = capacityList.Sum(size =>
            coworking.WorkspacesCapacity.Find(wc => wc.WorkspaceId == workspaceId)?.Availability
                .FirstOrDefault(r => r.Capacity == size)?.Amounts ?? 0);

        for (var slot = dayStart; slot < dayEnd; slot = slot.AddHours(MinSessionLength))
        {
            var slotEnd = slot.AddHours(MinSessionLength);
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


    private async Task<List<BookingModelDto>?> AddMultipleBooking(BookingModelDto bookingDto, WorkspaceModel workspace,
        CoworkingModel coworking)
    {
        var bookingList = new List<BookingModelDto>();

        foreach (var size in bookingDto.RoomSizes)
        {
            var booking = _mapper.Map<BookingModel>(bookingDto);
            booking.Id = Guid.NewGuid();
            booking.Workspace = workspace;
            booking.Coworking = coworking;
            booking.RoomSizes = [size];
            var entry = await _context.Booking.AddAsync(booking);
            bookingList.Add(_mapper.Map<BookingModelDto>(entry.Entity));
        }

        await _context.SaveChangesAsync();

        return bookingList;
    }

    private async Task<bool> CheckIfBookingAvailable(BookingModelDto bookingDto, CoworkingModel coworking)
    {
        foreach (var size in bookingDto.RoomSizes)
        {
            var maxRooms = coworking.WorkspacesCapacity
                .Find(w => w.WorkspaceId == bookingDto.WorkspaceId)?
                .Availability.Find(r => r.Capacity == size)?.Amounts ?? 0;

            var occupiedAmount = await _context.Booking.CountAsync(b =>
                b.Id != bookingDto.Id &&
                b.WorkspaceId == bookingDto.WorkspaceId &&
                b.CoworkingId == bookingDto.CoworkingId &&
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