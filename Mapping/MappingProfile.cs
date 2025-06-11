using AutoMapper;
using booking_api.Models;

namespace booking_api.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WorkspaceModel, WorkspaceModelDto>().ReverseMap();
        CreateMap<BookingModel, BookingModelDto>().ReverseMap();
        CreateMap<DateSlot, DateSlot>().ReverseMap();
        CreateMap<Availability, Availability>().ReverseMap();
        CreateMap<WorkspaceCapacity, WorkspaceCapacity>().ReverseMap();
        CreateMap<CoworkingModel, CoworkingModelDto>().ReverseMap();
    }
}