using AutoMapper;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;

namespace Palm.Mapper.Mapping;

public class MappingProfiler : Profile
{
    public MappingProfiler()
    {
        CreateMap<Session, SessionDto>()
            .ReverseMap();

        CreateMap<Session, SessionUpdateDto>()
            .ReverseMap();
        
        CreateMap<User, UserRegister>()
            .ForMember(options => options.Password, options => options.Ignore())
            .ReverseMap();

        CreateMap<Question, QuestionUpdateDto>()
            .ReverseMap();

        CreateMap<Answer, AnswerUpdateDto>()
            .ReverseMap();
        
        CreateMap<ICollection<Question>, ICollection<QuestionUpdateDto>>()
            .ReverseMap();
        
        CreateMap<ICollection<Answer>, ICollection<AnswerUpdateDto>>()
            .ReverseMap();
    }
}