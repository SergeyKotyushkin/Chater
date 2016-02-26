using System.Web.Mvc;
using Security.Contracts;
using Web.Models;

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
            var model = new HomeIndexModel {IsErrorVisible = false};
            return View(model);
        }

        public ActionResult Login(string textLogin, string textPassword)
        {
            var result = _security.Login(textLogin, textPassword);

            if (result)
                return RedirectToAction("Chat");


            

            return View("Index", new HomeIndexModel {IsErrorVisible = true});
        }

        public ActionResult Chat()
        {
            //_context.Clients.All.OnConnected("sergey", true);
            return View();
        }
    }
}
