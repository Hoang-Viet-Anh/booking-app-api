using AutoMapper;
using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Models;
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
        var isWorkspaceExist = await _context.Workspace.AnyAsync(w => w.Id == bookingDto.WorkspaceId);
        var coworking = await _context.Coworking.Include(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(cow => cow.Id == bookingDto.CoworkingId);

        if (!isWorkspaceExist || coworking == null) return null;

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, coworking);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto);

        return bookingList;
    }

    public async Task<List<BookingModelDto>?> EditBookingAsync(Guid id, BookingModelDto bookingDto)
    {
        var existingBooking = await _context.Booking
            .Include(b => b.Coworking)
            .ThenInclude(c => c.WorkspacesCapacity)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (existingBooking == null) return null;

        var isWorkspaceExist = await _context.Workspace.AnyAsync(w => w.Id == bookingDto.WorkspaceId);
        if (!isWorkspaceExist) return null;

        var isBookingAvailable = await CheckIfBookingAvailable(bookingDto, existingBooking.Coworking);
        if (!isBookingAvailable) return [];

        var bookingList = await AddMultipleBooking(bookingDto);
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

    public async Task<List<DateTime>> GetAllBookingDatesAsync(AvailabilityPropertiesDto availabilityProperties)
    {
        var bookings = await _context.Booking
            .Include(b => b.TimeSlots)
            .Where(b =>
                b.Id != availabilityProperties.BookingId &&
                b.CoworkingId == availabilityProperties.CoworkingId &&
                b.WorkspaceId == availabilityProperties.WorkSpaceId &&
                b.AreaCapacity.Intersect(availabilityProperties.CapacityList).Any()
            )
            .Select(b => new
            {
                b.StartDate,
                b.EndDate,
                b.AreaCapacity,
                b.TimeSlots,
                b.Coworking
            }).ToListAsync();

        var coworking = await _context.Coworking.FirstOrDefaultAsync(c => c.Id == availabilityProperties.CoworkingId);

        if (coworking == null) return [];

        var bookedDates = new HashSet<DateTime>();

        foreach (var capacity in availabilityProperties.CapacityList)
        {
            var filteredBookings = bookings.Where(b => b.AreaCapacity.Contains(capacity)).ToList();
            var maxAmount = coworking.WorkspacesCapacity
                .Find(wc => wc.WorkspaceId == availabilityProperties.WorkSpaceId)?.Availability
                .Find(a => a.Capacity == capacity)?.Amounts ?? 0;

            var datesWithBookings = new HashSet<DateTime>();
            datesWithBookings.Add(DateTime.UtcNow);

            foreach (var booking in filteredBookings)
            {
                for (var day = booking.StartDate; day <= booking.EndDate; day = day.AddDays(MinSessionLength))
                {
                    datesWithBookings.Add(day);
                }
            }

            var timeSlots = filteredBookings
                .SelectMany(fb => fb.TimeSlots)
                .Select(fb => new DateRange()
                {
                    startDate = fb.StartTime,
                    endDate = fb.EndTime,
                }).ToList();

            foreach (var date in datesWithBookings)
            {
                var availableTimeSlots = await GetDateTimeSlots(date, timeSlots, maxAmount);


                if (availableTimeSlots.Count == 0)
                {
                    bookedDates.Add(date.Date);
                }
            }
        }

        return bookedDates.ToList();
    }

    public async Task<List<DateRange>> GetAvailableTimeAsync(AvailabilityPropertiesDto availabilityProperties)
    {
        var startDate = availabilityProperties.DateRange.startDate.Date;
        var endDate = availabilityProperties.DateRange.endDate.Date;

        var daysAmount = endDate.Subtract(startDate).Days + 1;

        var bookings = await _context.Booking
            .Include(b => b.TimeSlots)
            .Where(b =>
                b.Id != availabilityProperties.BookingId &&
                b.CoworkingId == availabilityProperties.CoworkingId &&
                b.WorkspaceId == availabilityProperties.WorkSpaceId &&
                b.AreaCapacity.Intersect(availabilityProperties.CapacityList).Any() &&
                b.StartDate.Date <= endDate &&
                b.EndDate.Date >= startDate
            ).ToListAsync();

        var coworking = await _context.Coworking.FirstOrDefaultAsync(c => c.Id == availabilityProperties.CoworkingId);

        if (coworking == null) return [];

        var allTimeSlots = new List<DateRange>();

        foreach (var capacity in availabilityProperties.CapacityList)
        {
            var filteredBookings = bookings.Where(b => b.AreaCapacity.Contains(capacity)).ToList();
            var maxAmount = coworking.WorkspacesCapacity
                .Find(wc => wc.WorkspaceId == availabilityProperties.WorkSpaceId)?.Availability
                .Find(a => a.Capacity == capacity)?.Amounts ?? 0;

            var timeSlots = filteredBookings
                .SelectMany(fb => fb.TimeSlots)
                .Select(fb => new DateRange()
                {
                    startDate = fb.StartTime,
                    endDate = fb.EndTime,
                }).ToList();

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var availableTimeSlots = await GetDateTimeSlots(day, timeSlots, maxAmount);
                allTimeSlots.AddRange(availableTimeSlots);
            }
        }

        var commonTimeSlots = allTimeSlots
            .GroupBy(slot => slot)
            .Where(group => group.Count() == daysAmount * availabilityProperties.CapacityList.Count)
            .Select(group => group.Key)
            .ToList();


        return commonTimeSlots;
    }

    private async Task<List<DateRange>> GetDateTimeSlots(DateTime date, List<DateRange> bookedTimeSlots, int maxAmount)
    {
        var isToday = DateTime.UtcNow.Date == date.Date;
        var availableStartHour = isToday ? DateTime.UtcNow.Hour + 1 : StartHourUtc;

        if (availableStartHour >= EndHourUtc)
        {
            return [];
        }

        var startDate = new DateTime(date.Year, date.Month, date.Day, availableStartHour, 0, 0);
        var endDate = new DateTime(date.Year, date.Month, date.Day, EndHourUtc, 0, 0);

        var availableTimeSlots = new List<DateRange>();

        for (var day = startDate; day < endDate; day = day.AddHours(MinSessionLength))
        {
            var startTime = day;
            var endTime = day.AddHours(MinSessionLength);

            var occupiedCount = bookedTimeSlots.Count(ts =>
                ts.startDate < endTime &&
                ts.endDate > startTime
            );

            if (occupiedCount >= maxAmount)
            {
                continue;
            }

            availableTimeSlots.Add(new DateRange()
            {
                startDate = startTime,
                endDate = endTime,
            });
        }

        return availableTimeSlots;
    }

    private async Task<List<BookingModelDto>?> AddMultipleBooking(BookingModelDto bookingDto)
    {
        var bookingList = new List<BookingModelDto>();

        foreach (var size in bookingDto.AreaCapacity)
        {
            var booking = _mapper.Map<BookingModel>(bookingDto);
            booking.Id = Guid.NewGuid();
            booking.StartDate = DateTime.SpecifyKind(bookingDto.StartDate, DateTimeKind.Utc);
            booking.EndDate = DateTime.SpecifyKind(bookingDto.EndDate, DateTimeKind.Utc);
            booking.AreaCapacity = [size];

            var timeSlots = new List<BookingTimeSlotModel>();

            for (var day = bookingDto.StartDate; day <= bookingDto.EndDate; day = day.AddDays(1))
            {
                var startTime = new DateTime(
                    day.Year, day.Month, day.Day,
                    booking.StartDate.Hour, booking.StartDate.Minute, booking.StartDate.Second
                );
                var endTime = new DateTime(
                    day.Year, day.Month, day.Day,
                    booking.EndDate.Hour, booking.EndDate.Minute, booking.EndDate.Second
                );

                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

                _logger.LogError("startTime: " + startTime + " endTime: " + endTime);

                timeSlots.Add(new BookingTimeSlotModel()
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                });
            }

            timeSlots.ForEach(ts => _logger.LogError("timeSlotStart: " + ts.StartTime + " timeSlotEnd: " + ts.EndTime));

            booking.TimeSlots = timeSlots;

            var entry = await _context.Booking.AddAsync(booking);
            bookingList.Add(_mapper.Map<BookingModelDto>(entry.Entity));
        }

        await _context.SaveChangesAsync();

        return bookingList;
    }

    private async Task<bool> CheckIfBookingAvailable(BookingModelDto bookingDto, CoworkingModel coworking)
    {
        var bookings = await _context.Booking.Where(b =>
                b.Id != bookingDto.Id &&
                b.WorkspaceId == bookingDto.WorkspaceId &&
                b.CoworkingId == bookingDto.CoworkingId &&
                b.AreaCapacity.Intersect(bookingDto.AreaCapacity).Any() &&
                b.StartDate.Date <= bookingDto.EndDate.Date &&
                b.EndDate.Date >= bookingDto.StartDate.Date)
            .Include(b => b.TimeSlots)
            .Select(b => new
            {
                b.Id,
                b.AreaCapacity,
                b.TimeSlots
            })
            .ToListAsync();


        foreach (var capacity in bookingDto.AreaCapacity)
        {
            var maxAmount = coworking.WorkspacesCapacity.Find(wc => wc.WorkspaceId == bookingDto.WorkspaceId)?
                .Availability.Find(a => a.Capacity == capacity)?.Amounts ?? 0;

            var filteredBookings = bookings
                .Where(b => b.AreaCapacity.Contains(capacity))
                .ToList();

            for (var day = bookingDto.StartDate; day <= bookingDto.EndDate; day = day.AddDays(1))
            {
                var startTime = day;
                var endTime = new DateTime(
                    day.Year, day.Month, day.Day,
                    bookingDto.EndDate.Hour, bookingDto.EndDate.Minute, bookingDto.EndDate.Second
                );

                var occupiedCount = filteredBookings
                    .SelectMany(fb => fb.TimeSlots)
                    .Count(ts =>
                        ts.StartTime < endTime &&
                        ts.EndTime > startTime
                    );

                if (occupiedCount >= maxAmount)
                {
                    return false;
                }
            }
        }

        return true;
    }
}