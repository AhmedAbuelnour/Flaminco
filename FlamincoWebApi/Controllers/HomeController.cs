using Microsoft.AspNetCore.Mvc;

namespace FlamincoWebApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Json(new MyModel
            {
                SensitiveData = "Ahmed Ramadan"
            });
        }
    }
}
