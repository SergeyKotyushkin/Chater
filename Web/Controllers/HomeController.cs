using System.Web.Mvc;
using Security.Contracts;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISecurity _security;

        public HomeController(ISecurity security)
        {
            _security = security;
        }

        public ActionResult Index()
        {
            return View("Chat");
        }
    }
}
