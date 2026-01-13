using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace project_cripta.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}