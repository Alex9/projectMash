using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mash_v0._3.Models;
using System.Web.Security;

namespace Mash_v0._3.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private Models.databaseDataContext db= new databaseDataContext();

        public ActionResult Index()
        {
            var activeProject = (Project)Session["project"];

            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();
            try
            {
                var topics = (from n in db.Messages
                              where n.idProject == activeProject.idProject
                              orderby n.timeCreated descending
                              select n.idMessageTopic).Distinct().ToList(); ;

                ViewData.Add("topics", topics);
            }
            catch (Exception)
            {
            }

           // ViewData.Add("activeProject", activeProject.idProject);

            //Dohvati private poruke
            var getPrivateMessages = from n in db.PrivateMessages
                                     where n.idTo == signedUser.idUser
                                     orderby n.dateSent descending
                                     select n;
            

            if (getPrivateMessages.Count() != 0)
            {
                ViewData["privateMessages"] = getPrivateMessages.ToList();
            }
            else
            {
                ViewData["privateMessages"] = new List<PrivateMessage>();
            }

            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;
            return View();
        }
      
        public ViewResult SendPrivateMessage(string msgTo)
        {
            int id = Int32.Parse(msgTo);
            User forUser = (from n in db.Users
                            where n.idUser == id
                            select n).Single();
            ViewData["msgTo"] = forUser;
            return View();
        }

        [HttpPost]
        public RedirectResult SendPrivateMessage(string msgTo, string title, string message)
        {
            int idTo = Int32.Parse(msgTo);
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            PrivateMessage newPM = new PrivateMessage();
            newPM.dateSent = DateTime.Now;
            newPM.idFrom = signedUser.idUser;
            newPM.idMessageHeader = title;
            newPM.idMessageText = message;
            newPM.idTo = idTo;
            newPM.read = false;

            db.PrivateMessages.InsertOnSubmit(newPM);
            db.SubmitChanges();

            ViewBag.Message = "Message Sent";

            return Redirect("Index");
        }


        public ViewResult ShowPrivateMessage(string msgID)
        {
            int msg = Int32.Parse(msgID);

            PrivateMessage privateMessage = (from n in db.PrivateMessages
                                             where n.idPrivateMessage == msg
                                             select n).Single();
            privateMessage.read = true;

            User sender = (from n in db.Users
                           where n.idUser == privateMessage.idFrom
                           select n).Single();
            User reciver = (from n in db.Users
                            where n.idUser == privateMessage.idTo
                            select n).Single();

            db.SubmitChanges();

            ViewData["privateMessage"] = privateMessage;
            ViewData["sender"] = sender;
            ViewData["reciver"] = reciver;

            return View();
        }

        public PartialViewResult NewMessage()
        {
            return PartialView();
        }

        [HttpPost]
        public RedirectResult NewMessage(string to,string subject,string message)
        {
            User idTo=new User();
            try
            {
                idTo = db.Users.Where(item => item.email.Trim() == to).Single();
            }
            catch (Exception)
            {
                TempData["msgError"]="User dosen't exist";
                Redirect("Index");                 
            }
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            PrivateMessage newPM = new PrivateMessage();
            newPM.dateSent = DateTime.Now;
            newPM.idFrom = signedUser.idUser;
            newPM.idMessageHeader = subject;
            newPM.idMessageText = message;
            newPM.idTo = idTo.idUser;
            newPM.read = false;

            db.PrivateMessages.InsertOnSubmit(newPM);
            db.SubmitChanges();

            ViewBag.Message = "Message Sent";

            return Redirect("Index");
        }
    }
}
