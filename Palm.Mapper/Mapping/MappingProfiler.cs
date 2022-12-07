using AutoMapper;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;

namespace Palm.Mapper.Mapping;

public class MappingProfiler : Profile
{
    public MappingProfiler()
    {
        CreateMap<Session, SessionDto>();
        CreateMap<SessionDto, Session>();

        CreateMap<User, UserRegister>();
        CreateMap<UserRegister, User>();
    }
}