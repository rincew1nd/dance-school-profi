using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DanceSchool.Models
{
    public class IndexViewModel
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public byte[] Picture { get; set; }
        public IEnumerable<Registration> Registrations { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и пароль подтверждения не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}