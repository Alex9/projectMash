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
    public class UserRolesController : Controller
    {
        public databaseDataContext db = new databaseDataContext();
        //
        // GET: /UserRoles/

        public ActionResult Index()
        {
            Project activeProject = Session["project"] as Project;
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();
            ViewData["user"] = signedUser;
            if (activeProject != null)
            {
                var getProjectRolesList = from n in db.sifProjectUserRoles
                                         where n.idProject == activeProject.idProject
                                         select n;


                if (getProjectRolesList.Count() != 0)
                {
                    ViewData["rolesList"] = getProjectRolesList.ToList();
                }
                else
                {
                    ViewData["rolesList"] = null;
                }
            }

            return View();
        }

        //
        // GET: /UserRoles/Create


        public PartialViewResult Create()
        {
            return PartialView();
        } 

        //
        // POST: /UserRoles/Create

        [HttpPost]
        public ActionResult Create(sifProjectUserRole userRole)
        {
            try
            {
                var activeProject = Session["project"] as Project;
                sifProjectUserRole newUserRole = new sifProjectUserRole();

                newUserRole.idProject = activeProject.idProject;
                newUserRole.idUserRole = userRole.idUserRole;
                newUserRole.userRoleDescription = userRole.userRoleDescription;

                db.sifProjectUserRoles.InsertOnSubmit(newUserRole);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /UserRoles/Edit/5
 
        public ActionResult Edit(int id)
        {
            var userRole = db.sifProjectUserRoles.Where(p => p.idUserRole == id).First();
            return View(userRole);
        }

        //
        // POST: /UserRoles/Edit/5

        [HttpPost]
        public ActionResult Edit(sifProjectUserRole userRole)
        {
            try
            {
                var userRoleToEdit = db.sifProjectUserRoles.Where(p => p.idUserRole == userRole.idUserRole).First();
                userRoleToEdit.userRoleDescription = userRole.userRoleDescription;
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /UserRoles/Delete/5
 
        public ActionResult Delete(int id)
        {
            var userRole = db.sifProjectUserRoles.Where(p => p.idUserRole == id).First();
            return View(userRole);
        }

        //
        // POST: /UserRoles/Delete/5

        [HttpPost]
        public ActionResult Delete(sifProjectUserRole userRole)
        {
            try
            {
                var userRoleToDelete = db.sifProjectUserRoles.Where(p => p.idUserRole == userRole.idUserRole).First();

                Project activeProject = Session["project"] as Project;

                var existsUserWithRole = from n in db.sif_ProjectUsers
                                         join s in db.sifProjectUserRoles on n.idUserRole equals s.idUserRole
                                         where s.idProject == activeProject.idProject && s.idUserRole == userRoleToDelete.idUserRole
                                         select n;

                if (existsUserWithRole.Count() == 0)
                {
                    // ne postoji netko to ima tu user rolu
                    db.sifProjectUserRoles.DeleteOnSubmit(userRoleToDelete);
                    db.SubmitChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return Redirect("../ErrorOnDelete");
                }
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ErrorOnDelete()
        {
            return View();
        }


    }
}
