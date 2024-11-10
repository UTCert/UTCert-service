using AutoMapper;
using UTCert.Model.Database;
using UTCert.Model.Web.User;
using UTCert.Service.BusinessLogic.Dtos;

namespace UTCert.Service.BusinessLogic.Common;

public class AutoMapperProfile : Profile
{
    // mappings between model and entity objects
    public AutoMapperProfile()
    {
        CreateMap<RegisterDto, User>();
        CreateMap<User, UserResponseDto>();
        CreateMap<Certificate, CertificateDto>().ReverseMap();
        CreateMap<Contact, ContactDto>().ReverseMap();
    }
}