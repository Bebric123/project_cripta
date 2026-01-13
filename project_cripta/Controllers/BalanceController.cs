using project_cripta.Services;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace project_cripta.Controllers
{
    public class BalanceController : Controller
    {
        private readonly IBalanceService _balanceService;

        public BalanceController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var userId = (int?)Session["UserId"];
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var usdtBalance = await _balanceService.GetUserBalanceAsync(userId.Value, "USDT");
            var balanceAmount = usdtBalance?.Amount ?? 0m;

            return View(balanceAmount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddMoney(decimal amount)
        {
            var userId = (int?)Session["UserId"];
            if (userId == null)
                return Json(new { success = false, message = "Не авторизован" });

            if (amount <= 0)
                return Json(new { success = false, message = "Сумма должна быть больше 0" });

            try
            {
                var success = await _balanceService.UpdateBalanceAsync(userId.Value, "USDT", amount);

                if (success)
                {
                    var updatedBalance = await _balanceService.GetUserBalanceAsync(userId.Value, "USDT");
                    var newBalanceAmount = updatedBalance?.Amount ?? 0;

                    return Json(new
                    {
                        success = true,
                        newBalance = newBalanceAmount,
                        message = $"Счет пополнен на {amount:C2}"
                    });
                }

                return Json(new { success = false, message = "Ошибка пополнения счета" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddMoney error: {ex}");
                return Json(new { success = false, message = "Системная ошибка" });
            }
        }
    }
}