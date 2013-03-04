using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mash_v0._3.Models;
using System.Web.Security;
using System.Security.Permissions;

namespace Mash_v0._3.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {

        public databaseDataContext db = new databaseDataContext();

        public ViewResult AddNewProject()
        {
            return View();
        }

        [HttpPost]
        public RedirectResult AddNewProject(string projectName, string projectDescription)
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            Project newProject = new Project();
            newProject.projectName = projectName;
            newProject.description = projectDescription;
            newProject.idOwner = signedUser.idUser;
            newProject.dateCreated = DateTime.Now;


            db.Projects.InsertOnSubmit(newProject);
            db.SubmitChanges();

            sif_ProjectUser newMember = new sif_ProjectUser();
            newMember.idProject = newProject.idProject;
            newMember.idUser = signedUser.idUser;

            db.sif_ProjectUsers.InsertOnSubmit(newMember);
            db.SubmitChanges();
            return Redirect("/Dashboard/Index");
        }

        [Authorize]
        public RedirectToRouteResult LoadProject(string projectID)
        {
            Project activeProject;

            if (projectID == null)
            {
                activeProject = Session["project"] as Project;
            }
            else
            {
                int id = Int32.Parse(projectID);
                activeProject = db.Projects.Where(project => project.idProject == id).Single();
            }

            ViewBag.ProjectName = activeProject.projectName;
            ViewBag.ProjectDescription = activeProject.description;
            ViewBag.Title = activeProject.projectName;

            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            Session["project"] = activeProject;
            ViewData["project"] = activeProject;
            ViewData["user"] = signedUser;

            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        public RedirectResult SetActiveProject(string projectList)
        {
            string redirectUrl;

            if (Session["path"] != null)
            {
                redirectUrl = Session["path"] as string;
            }
            else
            {
                redirectUrl = "";
            }

            if (projectList == null)
                return Redirect(redirectUrl);

            int id = Int32.Parse(projectList);
            var activeProject = (from n in db.Projects
                                 where n.idProject == id
                                 select n).Single();

            Session["project"] = activeProject;
            return Redirect(redirectUrl);
        }


        public RedirectResult DeleteProject(string projectID)
        {
            int id = Int32.Parse(projectID);
            var membershipToDelete = db.sif_ProjectUsers.Where(project => project.idProject == id);
            db.sif_ProjectUsers.DeleteAllOnSubmit(membershipToDelete);

            var projectToDelete = db.Projects.Where(project => project.idProject == id);
            db.Projects.DeleteAllOnSubmit(projectToDelete);

            db.SubmitChanges();
            return Redirect("/Dashboard/Index");
        }

        //Za pregled svih projekata
        public ViewResult ListProject()
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            var projectList = from n in db.Projects
                              join s in db.sif_ProjectUsers on n.idProject equals s.idProject
                              where s.idUser == signedUser.idUser
                              select n;

            ViewData["user"] = signedUser;
            if (projectList.Count() != 0)
            {
                ViewData["projectList"] = projectList.ToList();
            }
            else
            {
                ViewData["projectList"] = null;
            }

            return View();
        }

        public PartialViewResult ShowInviteControle()
        {
            return PartialView();
        }

        public PartialViewResult InviteNewUser(string userEmail)
        {
            sif_InviteProjectUser newInvitation = new sif_InviteProjectUser();
            Project activeProject = Session["project"] as Project;
            User invitedUser;
            try
            {
                invitedUser = db.Users.Where(user => user.email.Trim() == userEmail.Trim()).Single();
            }
            catch (Exception)
            {
                TempData["ErrorInvitation"] = "User " + userEmail + " does not exist";
                return PartialView("InvitationStaus");
            }

            newInvitation.idProject = activeProject.idProject;
            newInvitation.idUser = invitedUser.idUser;

            var checkIfAlreadyInvited = from n in db.sif_InviteProjectUsers
                                        where n.idUser == invitedUser.idUser && n.idProject == activeProject.idProject
                                        select n;

            var checkIfAlreadyMember = from n in db.sif_ProjectUsers
                                       where n.idProject == activeProject.idProject && n.idUser == invitedUser.idUser
                                       select n;

            if (checkIfAlreadyInvited.Count() != 0)
            {
                TempData["ErrorInvitation"] = "User " + userEmail + " has already been invited on " + activeProject.projectName;
                return PartialView("InvitationStaus");
            }
            else if (checkIfAlreadyMember.Count() != 0)
            {
                TempData["ErrorInvitation"] = "User " + userEmail + " is already a member on " + activeProject.projectName;
                return PartialView("InvitationStaus");
            }
            else
            {
                db.sif_InviteProjectUsers.InsertOnSubmit(newInvitation);
                db.SubmitChanges();
                TempData["ErrorInvitation"] = "Invitation has been sent";
            }
            return PartialView("InvitationStaus");
        }

        public ViewResult ContactList()
        {
            Project activeProject = Session["project"] as Project;
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();
            ViewData["user"] = signedUser;
            if (activeProject != null)
            {
                var getMemberList = from n in db.Users
                                    join s in db.sif_ProjectUsers on n.idUser equals s.idUser
                                    where s.idProject == activeProject.idProject
                                    select n;

                var getMemberRolesList = from n in db.sifProjectUserRoles
                                         where n.idProject == activeProject.idProject
                                         select n;

                var getSifProjectUser = from n in db.sif_ProjectUsers
                                        where n.idProject == activeProject.idProject
                                        select n;

                ViewData["getSifProjectUser"] = getSifProjectUser.ToList();
                if (getMemberRolesList.Count() != 0)
                {
                    ViewData["rolesList"] = getMemberRolesList.ToList();
                }
                else
                {
                    ViewData["rolesList"] = null;
                }

                if (getMemberList.Count() != 0)
                {
                    ViewData["userList"] = getMemberList.ToList();
                }
                else
                {
                    ViewData["userList"] = null;
                }
            }
            else
            {
                ViewData["userList"] = null;
            }
            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;

            return View();
        }

        public RedirectResult RemoveUser(string userID)
        {
            int id = Int32.Parse(userID);
            var removeUser = (from n in db.sif_ProjectUsers
                              where n.idUser == id
                              select n).Single();
            db.sif_ProjectUsers.DeleteOnSubmit(removeUser);
            db.SubmitChanges();
            return Redirect("ContactList");
        }

        public RedirectResult SetUserRole(int? userRoleSelect, int? userId)
        {
            string redirectUrl;
            Project activeProject = Session["project"] as Project;

            if (Session["path"] != null)
            {
                redirectUrl = Session["path"] as string;
            }
            else
            {
                redirectUrl = "";
            }

            var changeUserRole = (from n in db.sif_ProjectUsers
                                  where n.idUser == userId
                                  && n.Project == activeProject
                                  select n).Single();
            changeUserRole.idUserRole = userRoleSelect;
            db.SubmitChanges();
            return Redirect(redirectUrl);
        }

        public RedirectResult LeaveProject(int projectID)
        {
             string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            var userList = from n in db.sif_ProjectUsers
                           where n.idProject == projectID && n.idUser==signedUser.idUser
                           select n;
            db.sif_ProjectUsers.DeleteAllOnSubmit(userList);
            db.SubmitChanges();
            return Redirect("ListProject");
        }

        public RedirectResult DeleteProjectForGood(int projectID)
        {
            var deleteSif = from n in db.sifProjectUserRoles
                            where n.idProject == projectID
                            select n;
            db.sifProjectUserRoles.DeleteAllOnSubmit(deleteSif);

            var deleteSif2 = from n in db.sifTaskUserProjects
                             where n.idProject == projectID
                             select n;

            db.sifTaskUserProjects.DeleteAllOnSubmit(deleteSif2);

            var deleteSif3 = from n in db.sif_ProjectUsers
                             where n.idProject == projectID
                             select n;
            db.sif_ProjectUsers.DeleteAllOnSubmit(deleteSif3);

            var deleteSif4 = from n in db.sif_ProjectFiles
                             where n.idProject == projectID
                             select n;
            db.sif_ProjectFiles.DeleteAllOnSubmit(deleteSif4);

            var getProject = from n in db.Projects
                             where n.idProject == projectID
                             select n;
            db.Projects.DeleteAllOnSubmit(getProject);
            db.SubmitChanges();
            return Redirect("ListProject");
        }
    }
}
