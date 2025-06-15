using booking_api.Models;

namespace booking_api.Interfaces;

public interface IBookingsService
{
    Task<List<BookingModelDto>> GetAllBookingAsync();

    Task<BookingModelDto?> GetBookingByIdAsync(Guid id);

    Task<List<BookingModelDto>?> AddBookingAsync(BookingModelDto booking);

    Task<List<BookingModelDto>?> EditBookingAsync(Guid id, BookingModelDto booking);

    Task<List<DateTime>> GetAllBookingDatesAsync(AvailabilityPropertiesDto availabilityProperties);

    Task<List<DateRange>> GetAvailableTimeAsync(AvailabilityPropertiesDto availabilityProperties);

    Task DeleteBookingByIdAsync(Guid id);
}