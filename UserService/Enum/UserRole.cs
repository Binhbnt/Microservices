using System.ComponentModel.DataAnnotations;

namespace UserService.Enums
{
    public enum UserRole
    {
        [Display(Name = "Quản trị viên")]
        Admin = 0,

        [Display(Name = "Quản lý cấp cao")]
        SuperUser = 1,

        [Display(Name = "Người dùng")]
        User = 2
    }
}