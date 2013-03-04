using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Mash_v0._3.Models;
using System.Web.Security;

namespace Mash_v0._3.Controllers
{
    public class LoginController : Controller
    {
        private Models.databaseDataContext db;

        public LoginController()
        {
            db = new databaseDataContext();
            db.CreateDatabase();
        }

        public ActionResult Enter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Enter(string email, string password)
        {
            Models.User user;

            try
            {
                user = (from u in db.Users
                        where u.email == email && u.password == password
                        select u).First();
            }
            catch (Exception)
            {
                ViewBag.LoginError = "LogIn Error";
                return View();
            }


            user.dateLastVisited = DateTime.Now;
            db.SubmitChanges();

            // remember me ponasanje
            FormsAuthentication.SetAuthCookie(user.email, false);
            //Session["user"] = korisnik;
            //Nakon logina postavi aktivni projekt

            User signedUser = user;
            Project activeProject;

            var projectList = from n in db.Projects
                              join s in db.sif_ProjectUsers on n.idProject equals s.idProject
                              where s.idUser == signedUser.idUser
                              select n;

            if (projectList.Count() != 0)
            {
                activeProject = projectList.First();
                Session["project"] = activeProject;
            }

            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult Exit()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Enter", "Login");
        }

        public ViewResult Registration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registration(string name, string lastName, string email, string password)
        {
            User newUser = new User();
            newUser.firstname = name;
            newUser.lastname = lastName;
            newUser.email = email;
            newUser.password = password;
            newUser.isVIP = false;
            newUser.dateLastVisited = DateTime.Now;
            newUser.dateRegistered = DateTime.Now;

            db.Users.InsertOnSubmit(newUser);
            db.SubmitChanges();

            FormsAuthentication.SetAuthCookie(newUser.email, false);
            return RedirectToAction("Index", "Dashboard");

        }
    }
}
