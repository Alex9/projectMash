using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Mash_v0._3.Models;

namespace Mash_v0._3.Controllers
{
    [Authorize]
    public class WallController : Controller
    {
        databaseDataContext db = new databaseDataContext();

        public ActionResult Index()
        {
            List<Message> messages = new List<Message>();
            var topics = new List<int?>();
            var activeProject = (Project)Session["project"];

            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            //ovo vrati sve poruke vezane za projekt
            var getMessages = db.Messages.Where(msg => msg.idProject == activeProject.idProject);

            //sad tu treba provjeriti da li ima poruka
            try
            {
                if (getMessages.Count() != 0)
                {
                    messages = getMessages.ToList();
                }

            }
            catch(Exception)
            {

            }
            //i sad je pretraživanje puno brže
            foreach (var item in messages)
            {
                if (!topics.Contains(item.idMessageTopic))
                {
                    topics.Add(item.idMessageTopic);
                }
            }
            //ViewData.Add("activeProject", Session["project"]);

            ViewData.Add("messages", messages);
            ViewData.Add("topics", topics);
            //ViewData.Add("activeProject", activeProject.idProject);

            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;
            return View();
        }


        public PartialViewResult AddNewTopic()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddNewTopic(string topicMessage)
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            Project activeProject = Session["project"] as Project;

            Message newTopic = new Message();
            newTopic.idMessageAuthor = signedUser.idUser;
            newTopic.idProject = activeProject.idProject;
            newTopic.messageContent = topicMessage;
            newTopic.timeCreated = DateTime.Now;

            //Ubaci u bazu, da se pridjeli idMessage
            db.Messages.InsertOnSubmit(newTopic);
            db.SubmitChanges();

            //Zatim da id pridjeli idTopic jer se radni o novom topicu
            newTopic.idMessageTopic = newTopic.idMessage;
            db.SubmitChanges();

            return Redirect("Index");
        }

        public ActionResult ReplyToTopic(string topicID)
        {
            ViewData["topicID"] = Int32.Parse(topicID);
            return PartialView();
        }

        public RedirectResult UpdateReply(string topicID, string messageText)
        {
            int id = Int32.Parse(topicID);
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            Project activeProject = Session["project"] as Project;

            Message replyToTopic = new Message();
            replyToTopic.idMessageAuthor = signedUser.idUser;
            replyToTopic.idMessageTopic = id;
            replyToTopic.idProject = activeProject.idProject;
            replyToTopic.messageContent = messageText;
            replyToTopic.timeCreated = DateTime.Now;

            db.Messages.InsertOnSubmit(replyToTopic);
            db.SubmitChanges();

            return Redirect("OpenThread?topicID=" + topicID);
        }

        public ViewResult OpenThread(string topicID)
        {
            int id = Int32.Parse(topicID);
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            Project activeProject = Session["project"] as Project;

            var messageOnTopic = from n in db.Messages
                                 where n.idMessageTopic == id
                                 select n;
            try
            {
                if (messageOnTopic.Count() != 0)
                {
                    ViewData["messageOnTopic"] = messageOnTopic.ToList();
                }

                else
                {

                }
            }
            catch (Exception)
            {
                ViewData["messageOnTopic"] = null;
            }

            var listUser = from n in db.Users
                                  join s in db.sif_ProjectUsers on n.idUser equals s.idUser
                                  where s.idProject == activeProject.idProject
                                  select n;
            if (listUser.Count() > 0)
            {
                ViewData["userListProject"] = listUser.ToList();
            }
            else
            {
                ViewData["userListProject"] = null;
            }


            return View();
        }

    }
}
