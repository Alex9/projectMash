using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mash_v0._3.Controllers
{
    public class VisitorsController : Controller
    {
        //
        // GET: /Visitors/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AboutProjectMash()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }
    }
}
