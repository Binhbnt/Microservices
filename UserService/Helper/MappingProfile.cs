using AutoMapper;
using UserService.DTOs;
using UserService.Models; // Đảm bảo import User model
using System; // Để dùng Enum

namespace UserService.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ... (các ánh xạ đã có: User -> UserReadDto, UserCreateDto -> User, UserUpdateDto -> User) ...

            // Ánh xạ từ User Model sang LoginResponseDto (sau khi đăng nhập thành công)
            CreateMap<User, UserDetailDto>()
                    .ForMember(dest => dest.MaSoNhanVien, opt => opt.MapFrom(src => src.MaSoNhanVien))
                    .ForMember(dest => dest.HoTen, opt => opt.MapFrom(src => src.HoTen))
                    .ForMember(dest => dest.BoPhan, opt => opt.MapFrom(src => src.BoPhan))
                    .ForMember(dest => dest.ChucVu, opt => opt.MapFrom(src => src.ChucVu))
                    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
            CreateMap<UserReadDto, UserDetailDto>();
            CreateMap<User, LoginResponseDto>()
                    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                    .ForMember(dest => dest.Token, opt => opt.Ignore()); // Token sẽ được set sau khi map
        }
    }
}