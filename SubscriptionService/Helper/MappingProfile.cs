// File: Helper/MappingProfile.cs
using AutoMapper;
using SubscriptionService.Dtos;
using SubscriptionService.Models;

namespace SubscriptionService.Helper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Ánh xạ từ Model -> DTO (để trả về cho client)
        CreateMap<SubscribedService, SubscriptionReadDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate.ToString("dd/MM/yyyy")))
            // Đây là logic tính số ngày còn lại
            .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src => (src.ExpiryDate.Date - DateTime.UtcNow.Date).Days));

        // Ánh xạ từ DTO -> Model (để lưu vào database)
        CreateMap<SubscriptionCreateDto, SubscribedService>();
        CreateMap<SubscriptionUpdateDto, SubscribedService>();
    }
}