using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Mash_v0._3.Models;

namespace Mash_v0._3.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        databaseDataContext db = new databaseDataContext();

        public DashboardController()
        {
        }

        public void CreateTable(Type linqTableClass)
        {
            using (var tempDc = new databaseDataContext())
            {
                var metaTable = tempDc.Mapping.GetTable(linqTableClass);
                var typeName = "System.Data.Linq.SqlClient.SqlBuilder";
                var type = typeof(DataContext).Assembly.GetType(typeName);
                var bf = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
                var sql = type.InvokeMember("GetCreateTableCommand", bf, null, null, new[] { metaTable });
                var sqlAsString = sql.ToString();
                tempDc.ExecuteCommand(sqlAsString);
            }
        }
        public ActionResult Index()
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();
            //Project activeProject;

            //var projectList = from n in db.Projects
            //                  join s in db.sif_ProjectUsers on n.idProject equals s.idProject
            //                  where s.idUser==signedUser.idUser
            //                  select n;

            //if (projectList.Count() != 0)
            //{
            //    activeProject = projectList.First();
            //    Session["project"] = activeProject;
            //}

            //projeveri je li ima pozivnica
            var newInvitation = (from n in db.sif_InviteProjectUsers
                                 where n.idUser == signedUser.idUser
                                 select n.idProject);
            if (newInvitation.Count() != 0)
            {
                var projectInvite = from n in db.Projects
                                    where newInvitation.ToList().Contains(n.idProject)
                                    select n;

                if (projectInvite.Count() != 0)
                {
                    ViewData["invitations"] = projectInvite.ToList();
                }
                else
                {
                    ViewData["invitations"] = null;
                }
            }
            else
            {
                ViewData["invitations"] = null;
            }

            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;

            PopulateLiveFeed(signedUser);

            return View();
        }

        public void PopulateLiveFeed(User signedUser)
        {
            //Dohvati sve projekte na kojima sudjeluje korisnik
            var projectList=from n in db.sif_ProjectUsers
                            where n.idUser == signedUser.idUser
                            select n.idProject;
            if(projectList.Count()==0) return;

            //Dohvati zadnje fajlove za sve projekte na kojima sudjeluje korisnik
            var getFileList = from n in db.Files
                              join a in db.sif_ProjectFiles on n.idFile equals a.idFile
                              where projectList.Contains(a.idProject)
                              orderby n.dateCreated descending
                              select n;
            if (getFileList.Count() > 0)
                ViewData["files"] = getFileList.ToList();

            //Dohvati zadnje poruke za sve projekte na kojima sudjeluje korisnik
        }

        public RedirectResult AcceptInvitation(string projectID)
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            int projectId = Int32.Parse(projectID);

            sif_InviteProjectUser invitation = (from n in db.sif_InviteProjectUsers
                                                where n.idProject == projectId && n.idUser == signedUser.idUser
                                                select n).Single();
            sif_ProjectUser newMembership = new sif_ProjectUser();
            newMembership.idUser = signedUser.idUser;
            newMembership.idProject = projectId;

            db.sif_InviteProjectUsers.DeleteOnSubmit(invitation);
            db.sif_ProjectUsers.InsertOnSubmit(newMembership);
            db.SubmitChanges();

            return Redirect("Index");
        }

        public RedirectResult DeclineInvitation(string projectID)
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            int projectId = Int32.Parse(projectID);

            sif_InviteProjectUser invitation = (from n in db.sif_InviteProjectUsers
                                                where n.idProject == projectId && n.idUser == signedUser.idUser
                                                select n).Single();

            db.sif_InviteProjectUsers.DeleteOnSubmit(invitation);
            db.SubmitChanges();

            return Redirect("Index");
        }
    }
}
