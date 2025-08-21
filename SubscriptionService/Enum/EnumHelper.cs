using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SubscriptionService.DTOs;

namespace SubscriptionService.Enums;

public static class EnumHelper
{
    public static List<EnumDto> GetEnumDisplayList<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
                   .Cast<T>()
                   .Select(e => new EnumDto
                   {
                       Key = Convert.ToInt32(e), // "Key" bây giờ là số
                       DisplayName = e.GetType()
                                      .GetMember(e.ToString())
                                      .First()
                                      .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                   })
                   .ToList();
    }
    public static string GetDisplayName(Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        ?.GetName() ?? enumValue.ToString();
    }
}