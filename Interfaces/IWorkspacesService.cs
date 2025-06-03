using booking_api.Models;

namespace booking_api.Interfaces;

public interface IWorkspacesService
{
    Task<List<WorkspaceModelDto>> GetAllWorkspacesAsync();
}