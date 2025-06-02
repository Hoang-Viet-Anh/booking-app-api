using Microsoft.EntityFrameworkCore;

namespace booking_api.Models;

public class WorkspaceModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required List<string> Amenities { get; set; }
    public required Availability Availability { get; set; }
    public required int MaxBookingDays { get; set; }
}

public class WorkspaceModelDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required List<string> Amenities { get; set; }
    public required Availability Availability { get; set; }
    public required int MaxBookingDays { get; set; }
}

[Owned]
public class Availability
{
    public required string Type { get; set; }
    public required List<Room> Rooms { get; set; }
}

[Owned]
public class Room
{
    public required int RoomsAmount { get; set; }
    public required int Capacity { get; set; }
}