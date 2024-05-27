using AutoMapper;
using UTCert.Model.Database;
using UTCert.Model.Web.User;

namespace UTCert.Service.Helper.Common;

public class AutoMapperProfile : Profile
{
    // mappings between model and entity objects
    public AutoMapperProfile()
    {
        CreateMap<RegisterDto, User>();

        CreateMap<User, UserResponseDto>();
        //
        // CreateMap<RegisterRequest, Account>();
        //
        // CreateMap<CreateRequest, Account>();
        //
        // CreateMap<UpdateRequest, Account>()
        //     .ForAllMembers(x => x.Condition(
        //         (src, dest, prop) =>
        //         {
        //             // ignore null & empty string properties
        //             if (prop == null) return false;
        //             if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;
        //
        //             // ignore null role
        //             if (x.DestinationMember.Name == "Role" && src.Role == null) return false;
        //
        //             return true;
        //         }
        //     ));
    }
}