using Microsoft.EntityFrameworkCore;

namespace booking_api.Models;

public class BookingModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public CoworkingModel Coworking { get; set; } = null!;
    public required Guid CoworkingId { get; set; }
    public WorkspaceModel Workspace { get; set; } = null!;
    public required Guid WorkspaceId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required List<int> AreaCapacity { get; set; }

    public ICollection<BookingTimeSlotModel> TimeSlots { get; set; } = new List<BookingTimeSlotModel>();
}

public class BookingModelDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required Guid CoworkingId { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required List<int> AreaCapacity { get; set; }
}