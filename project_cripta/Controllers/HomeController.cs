using project_cripta.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace project_cripta.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICryptoService _cryptoService;

        public HomeController(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }
        public async Task<ActionResult> Index(int page = 1, int perPage = 100)
        {
            var cryptocurrencies = await _cryptoService.GetCryptoCurrenciesAsync(page, perPage);

            ViewBag.CurrentPage = page;
            ViewBag.PerPage = perPage;
            ViewBag.TotalPages = 10;

            return View(cryptocurrencies);
        }
    }
}