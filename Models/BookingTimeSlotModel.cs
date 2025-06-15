namespace booking_api.Models;

public class BookingTimeSlotModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid BookingId { get; set; }
    public BookingModel Booking { get; set; } = null!;
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
}

public class AvailabilityPropertiesDto
{
    public Guid? BookingId { get; set; }
    public required Guid WorkSpaceId { get; set; }
    public required Guid CoworkingId { get; set; }
    public required List<int> CapacityList { get; set; }
    public DateRange? DateRange { get; set; }
}

public class DateRange
{
    public required DateTime startDate { get; set; }
    public required DateTime endDate { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not DateRange other)
            return false;

        return startDate.Hour == other.startDate.Hour && endDate.Hour == other.endDate.Hour;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(startDate.Hour, endDate.Hour);
    }
}