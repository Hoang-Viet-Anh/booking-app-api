using Microsoft.EntityFrameworkCore;

namespace booking_api.Models;

public class CoworkingModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required List<WorkspaceCapacity> WorkspacesCapacity { get; set; }
}

public class CoworkingModelDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required List<WorkspaceCapacity> WorkspacesCapacity { get; set; }
}

[Owned]
public class WorkspaceCapacity
{
    public required Guid WorkspaceId { get; set; }
    public required List<Availability> Availability { get; set; }
}

[Owned]
public class Availability
{
    public required int Amounts { get; set; }
    public required int Capacity { get; set; }
}