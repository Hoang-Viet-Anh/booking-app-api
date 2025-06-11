using Microsoft.EntityFrameworkCore;

namespace booking_api.Models;

public class BookingModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required CoworkingModel Coworking { get; set; }
    public required Guid CoworkingId { get; set; }
    public required WorkspaceModel Workspace { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required DateSlot DateSlot { get; set; }
    public required List<int> RoomSizes { get; set; }
}

public class BookingModelDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required Guid CoworkingId { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required DateSlot DateSlot { get; set; }
    public required List<int> RoomSizes { get; set; }
}

[Owned]
public class DateSlot
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required bool IsStartTimeSelected { get; set; }
    public required bool IsEndTimeSelected { get; set; }
}

public class AvailableTimesDto
{
    public List<DateTime> StartTimes { get; set; } = [];
    public List<DateTime> EndTimes { get; set; } = [];
}