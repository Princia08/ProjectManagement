using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ReportMeeting.Models;
using ReportMeeting.Services;

namespace ReportMeeting.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check session for user info and set ViewBag.User accordingly
            var user = HttpContext.Session.GetObject<Users>("User");
            if (user != null)
            {
                ViewBag.User = user;
            }

            base.OnActionExecuting(context);
        }
    }
}
