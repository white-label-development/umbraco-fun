using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AngUm.Controllers
{
    public class AngularAppController : Umbraco.Web.Mvc.RenderMvcController
    {
        public ActionResult AngularAppView()
        {
            return View();
        }

        public ActionResult Hello()
        {
            return View();
        }
    }
}
