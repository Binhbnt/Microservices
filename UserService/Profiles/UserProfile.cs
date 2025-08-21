using UserService.Models;
using AutoMapper;
using UserService.DTOs;

namespace UserService.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Dạy cho AutoMapper cách dịch:
            // Nguồn: User, Đích: UserReadDto
            CreateMap<User, UserReadDto>()
                // Vì Role trong User là enum, trong DTO là string, ta cần chỉ rõ cách dịch
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // Nguồn: UserCreateDto, Đích: User
            CreateMap<UserCreateDto, User>();

            // Nguồn: UserUpdateDto, Đích: User
            CreateMap<UserUpdateDto, User>();
        }
    }
}