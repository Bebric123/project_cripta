using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using project_cripta.Models;
using project_cripta.Services;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace project_cripta.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.AuthenticateUserAsync(model);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            Session["UserId"] = user.Id;
            Session["UserName"] = $"{user.FirstName}";
            Session["UserEmail"] = user.Email;

            if (model.RememberMe)
            {
                var authCookie = new HttpCookie("UserAuth", user.Id.ToString())
                {
                    Expires = DateTime.Now.AddDays(30)
                };
                Response.Cookies.Add(authCookie);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userService.UserExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                return View(model);
            }

            var result = await _userService.RegisterUserAsync(model);
            if (result)
            {
                var loginModel = new LoginViewModel
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var user = await _userService.AuthenticateUserAsync(loginModel);

                Session["UserId"] = user.Id;
                Session["UserName"] = $"{user.FirstName}";
                Session["UserEmail"] = user.Email;

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Произошла ошибка при регистрации");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["UserAuth"] != null)
            {
                var cookie = new HttpCookie("UserAuth")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}