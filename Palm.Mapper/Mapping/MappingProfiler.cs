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

        CreateMap<Session, SessionUpdateDto>();
        CreateMap<SessionUpdateDto, Session>();
        
        CreateMap<User, UserRegister>();
        CreateMap<UserRegister, User>();

        CreateMap<Question, QuestionUpdateDto>();
        CreateMap<QuestionUpdateDto, Question>();

        CreateMap<Answer, AnswerUpdateDto>();
        CreateMap<AnswerUpdateDto, Answer>();
        
        CreateMap<ICollection<Question>, ICollection<QuestionUpdateDto>>();
        CreateMap<ICollection<QuestionUpdateDto>, ICollection<Question>>();
        
        CreateMap<ICollection<Answer>, ICollection<AnswerUpdateDto>>();
        CreateMap<ICollection<AnswerUpdateDto>, ICollection<Answer>>();
    }
}