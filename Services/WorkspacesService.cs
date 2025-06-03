using AutoMapper;
using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Services;

public class WorkspacesService : IWorkspacesService
{
    private readonly BookingContext _dbContext;
    private readonly IMapper _mapper;

    public WorkspacesService(BookingContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<List<WorkspaceModelDto>> GetAllWorkspacesAsync()
    {
        var workspaces = await _dbContext.Workspace.ToListAsync();
        return _mapper.Map<List<WorkspaceModelDto>>(workspaces);
    }
}