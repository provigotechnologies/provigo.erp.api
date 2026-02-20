using AutoMapper;
using IdentityService.DTOs;
using IdentityService.Models;

namespace IdentityService.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserCreateRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // PasswordHash is handled manually

            CreateMap<UserUpdateRequest, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // Only map non-null fields

            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.FullName,
                           opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        }
    }

}
