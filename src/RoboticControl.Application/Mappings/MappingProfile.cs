using AutoMapper;
using RoboticControl.Application.DTOs;
using RoboticControl.Domain.Entities;

namespace RoboticControl.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RobotPosition, RobotPositionDto>();
        CreateMap<RobotPositionDto, RobotPosition>();

        CreateMap<RobotStatus, RobotStatusDto>()
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()));

        CreateMap<WorkEnvelope, WorkEnvelopeDto>();
        CreateMap<WorkEnvelopeDto, WorkEnvelope>();

        CreateMap<MoveCommandDto, RobotPosition>()
            .ForMember(dest => dest.RotationX, opt => opt.MapFrom(src => src.RotationX ?? 0))
            .ForMember(dest => dest.RotationY, opt => opt.MapFrom(src => src.RotationY ?? 0))
            .ForMember(dest => dest.RotationZ, opt => opt.MapFrom(src => src.RotationZ ?? 0))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
