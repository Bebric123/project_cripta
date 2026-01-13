using System.ComponentModel.DataAnnotations;

namespace project_cripta.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}