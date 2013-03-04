using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mash_v0._3.Models;

namespace Mash_v0._3.Controllers
{
    public class TagController : Controller
    {
        private databaseDataContext db = new databaseDataContext();
        public ActionResult TagSelector(string tagName)
        {
            var getTagID = (from n in db.Tags
                           where n.tagName==tagName
                           select n.idTag).Single();

            var getTaggedFiles = from n in db.Files
                                 join s in db.sif_TagFiles on n.idFile equals s.idFile
                                 where s.idTag == getTagID
                                 select n;

            if (getTaggedFiles.Count() > 0)
            {
                ViewData["files"] = getTaggedFiles.ToList();
            }

            //Dodati istu stvar i za taskove koji se mogu tagati
            var getTaggedTask = from n in db.Tasks
                                join s in db.sifUserTasks on n.idTask equals s.idTask
                                where s.idTag == getTagID
                                select n;

            if (getTaggedTask.Count() > 0)
            {
                ViewData["task"] = getTaggedTask.ToList();
            }

            return View();
        }

    }
}
