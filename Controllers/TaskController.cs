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
    public class TaskController : Controller
    {

        //private Models.mash_PrimarnaBazaEntities1 data;
        private Models.databaseDataContext db;

        public TaskController()
        {
            db = new databaseDataContext();
        }

        public ActionResult Index()
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            var activeProject = Session["project"] as Project;

            var tasks = from n in db.Tasks
                        join s in db.sifTaskUserProjects on n.idTask equals s.idTask
                        where /*s.idUser == signedUser.idUser &&*/ s.idProject == activeProject.idProject
                        select n;
            try
            {
                if (tasks.Count() != 0)
                {
                    ViewData["tasks"] = tasks.ToList();
                }
                else
                {
                    ViewData["tasks"] = null;
                }
            }
            catch (Exception) { }
            Dictionary<int, List<User>> taskTagged = new Dictionary<int, List<Models.User>>();

            //var taggedPersons = from n in db.Tasks
            //                    join a in db.sifTaskUserProjects on n.idTask equals a.idTask
            //                    where a.idProject == activeProject.idProject
            //                    select n;

            var users = from n in db.Users
                        join a in db.sif_ProjectUsers on n.idUser equals a.idUser
                        where a.idProject == activeProject.idProject
                        select n;
            var sifUserTag = from n in db.sifUserTasks
                             where n.idUser != 0
                             select n;

            foreach (var item in tasks)
            {
                foreach (var usertag in sifUserTag)
                {
                    if (usertag.idTask == item.idTask)
                    {
                        if (taskTagged.ContainsKey(item.idTask))
                        {
                            taskTagged[item.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                        }
                        else
                        {
                            taskTagged.Add(item.idTask, new List<User>());
                            taskTagged[item.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                        }
                    }
                }
            }

            if (taskTagged.Count > 0)
            {
                ViewData["taskTagged"] = taskTagged;
            }




            //Prilikom promjene projekta ova varijabla govori gdje smo prethodno bili
            Session["path"] = Request.Path;

            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Task task)
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            var activeProject = Session["project"] as Project;

            if (!ModelState.IsValid)
            {
                //return View(task);
            }
            // primary key constraint error!

            db.Tasks.InsertOnSubmit(task);
            db.SubmitChanges();

            sifTaskUserProject newTaskEntry = new sifTaskUserProject();
            newTaskEntry.idProject = activeProject.idProject;
            newTaskEntry.idUser = signedUser.idUser;
            newTaskEntry.idTask = task.idTask;
            db.sifTaskUserProjects.InsertOnSubmit(newTaskEntry);

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult CreateAjax()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult CreateAjax(Task task, string datePicker)
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            var activeProject = Session["project"] as Project;

            if (!ModelState.IsValid)
            {
                //return View(task);
            }
            // primary key constraint error!

            task.taskDeadline = DateTime.Parse(datePicker);
            db.Tasks.InsertOnSubmit(task);
            
            db.SubmitChanges();

            sifTaskUserProject newTaskEntry = new sifTaskUserProject();
            newTaskEntry.idProject = activeProject.idProject;
            newTaskEntry.idUser = signedUser.idUser;
            newTaskEntry.idTask = task.idTask;
            db.sifTaskUserProjects.InsertOnSubmit(newTaskEntry);

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var task = db.Tasks.Where(p => p.idTask == id).First();

            return View(task);
        }

        [HttpPost]
        public ActionResult Edit(Task task)
        {
            var taskToEdit = db.Tasks.Where(p => p.idTask == task.idTask).First();
            taskToEdit.taskHeader = task.taskHeader;
            taskToEdit.description = task.description;
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var task = db.Tasks.Where(p => p.idTask == id).First();

            return View(task);
        }

        [HttpPost]
        public ActionResult Delete(string idTask)
        {
            int idTaskToDelete = Int32.Parse(idTask);
            var taskToDelete = db.Tasks.Where(p => p.idTask == idTaskToDelete).First();

            sifTaskUserProject deleteTaskEntry = (from n in db.sifTaskUserProjects
                                                  where n.idTask == idTaskToDelete
                                                  select n).Single();
            db.sifTaskUserProjects.DeleteOnSubmit(deleteTaskEntry);
            db.Tasks.DeleteOnSubmit(taskToDelete);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            var activeProject = Session["project"] as Project;

            var task = (from n in db.Tasks
                        where n.idTask == id
                        select n).Single();

            ViewData["task"] = task;
            ViewData["user"] = signedUser;

            Dictionary<int, List<User>> taskTagged = new Dictionary<int, List<Models.User>>();

            var users = from n in db.Users
                        join a in db.sif_ProjectUsers on n.idUser equals a.idUser
                        where a.idProject == activeProject.idProject
                        select n;

            var sifUserTag = from n in db.sifUserTasks
                             where n.idUser != 0
                             select n;


            foreach (var usertag in sifUserTag)
            {
                if (usertag.idTask == task.idTask)
                {
                    if (taskTagged.ContainsKey(task.idTask))
                    {
                        taskTagged[task.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                    }
                    else
                    {
                        taskTagged.Add(task.idTask, new List<User>());
                        taskTagged[task.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                    }
                }
            }

            if (taskTagged.Count > 0)
            {
                ViewData["taskTagged"] = taskTagged;
            }

            return View(task);
        }

        [HttpPost]
        public ActionResult Details(string idTask)
        {
            int idTaskToDelete = Int32.Parse(idTask);
            var taskToDelete = db.Tasks.Where(p => p.idTask == idTaskToDelete).First();
            db.Tasks.DeleteOnSubmit(taskToDelete);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public RedirectResult AddUserTask(string idTask, string selectUser)
        {
            sifUserTask TagUser = new sifUserTask()
            {
                idTask = Int32.Parse(idTask),
                idUser = Int32.Parse(selectUser)
            };

            db.sifUserTasks.InsertOnSubmit(TagUser);
            db.SubmitChanges();

            return Redirect("Index");
        }

        public PartialViewResult ShowTaskTagEditControl(string idTask)
        {
            int id = Int32.Parse(idTask);

            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            var activeProject = Session["project"] as Project;

            var task = (from n in db.Tasks
                        where n.idTask == id
                        select n).Single();

            ViewData["task"] = task;
            ViewData["user"] = signedUser;

            Dictionary<int, List<User>> taskTagged = new Dictionary<int, List<Models.User>>();

            var users = from n in db.Users
                        join a in db.sif_ProjectUsers on n.idUser equals a.idUser
                        where a.idProject == activeProject.idProject
                        select n;

            var sifUserTag = from n in db.sifUserTasks
                             where n.idUser != 0
                             select n;

            foreach (var usertag in sifUserTag)
            {
                if (usertag.idTask == task.idTask)
                {
                    if (taskTagged.ContainsKey(task.idTask))
                    {
                        taskTagged[task.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                    }
                    else
                    {
                        taskTagged.Add(task.idTask, new List<User>());
                        taskTagged[task.idTask].Add(users.Where(user => user.idUser == usertag.idUser).Single());
                    }
                }
            }

            if (taskTagged.Count > 0)
            {
                ViewData["taskTagged"] = taskTagged;
            }

            var usersOnProject = from n in db.Users
                                 join a in db.sif_ProjectUsers on n.idUser equals a.idUser
                                 where a.idProject == activeProject.idProject
                                 select n;
            ViewData["usersOnProject"] = usersOnProject.ToList();

            return PartialView();
        }

        public RedirectResult AddingNewTaggedUserToTask(string[] selectedObjects, string idTask)
        {
            List<sifUserTask> newTags = new List<sifUserTask>();
            foreach (var item in selectedObjects)
            {
                sifUserTask tagTask = new sifUserTask();
                tagTask.idUser = Int32.Parse(item);
                tagTask.idTask = Int32.Parse(idTask);
                newTags.Add(tagTask);
            }

            db.sifUserTasks.InsertAllOnSubmit(newTags);
            db.SubmitChanges();

            return Redirect("Index");
        }

        public RedirectResult DeleteTag(string[] selectedObjects, string idTask)
        {
            List<sifUserTask> deleteList = new List<sifUserTask>();
            foreach (var item in selectedObjects)
            {
                int taskID = Int32.Parse(idTask);
                int idUser = Int32.Parse(item);
                sifUserTask forDeletion = (from n in db.sifUserTasks
                                           where n.idUser == idUser && n.idTask == taskID
                                           select n).Single();
                deleteList.Add(forDeletion);
            }
            db.sifUserTasks.DeleteAllOnSubmit(deleteList);
            db.SubmitChanges();
            return Redirect("Index");
        }

        public RedirectResult ApplyForTask(string idtask)
        {
            string emailFrom = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == emailFrom).Single();

            int idTask = Int32.Parse(idtask);

            sifUserTask tagTask = new sifUserTask();
            tagTask.idUser = signedUser.idUser;
            tagTask.idTask = idTask;

            db.sifUserTasks.InsertOnSubmit(tagTask);
            db.SubmitChanges();

            return Redirect("Index");
        }

        public PartialViewResult ShowTagControl(int idTask)
        {
            ViewData["TaskID"] = idTask;
            return PartialView();
        }

        public RedirectResult AddTag(int TaskID, string tagArray)
        {
            string[] tags = tagArray.Split(' ');

            foreach (var item in tags)
            {
                if (item == "") continue;
                Tag newTag = new Tag();

                var getTag = from n in db.Tags
                             where n.tagName.Trim() == item
                             select n;
                if (getTag.Count() == 0)
                {
                    newTag.tagName = item;
                    db.Tags.InsertOnSubmit(newTag);
                    db.SubmitChanges();

                    sifUserTask tagToAdd = new sifUserTask()
                    {
                        idTag = newTag.idTag,
                        idTask = TaskID
                    };
                    db.sifUserTasks.InsertOnSubmit(tagToAdd);
                    db.SubmitChanges();
                }
                else
                {
                    sifUserTask tagToAdd = new sifUserTask()
                    {
                        idTag = getTag.First().idTag,
                        idTask = TaskID
                    };
                    db.sifUserTasks.InsertOnSubmit(tagToAdd);
                    db.SubmitChanges();
                }
            }

            return Redirect("Index");
        }

        public RedirectResult DeleteTagSearch(string[] tagCollection, string TaskID2)
        {
            List<sifUserTask> deleteList = new List<sifUserTask>();
            int idTask = Int32.Parse(TaskID2);
            foreach (var item in tagCollection)
            {
                var getTag = (from n in db.Tags
                             where n.tagName.Trim() == item
                             select n).Single();
                sifUserTask forDeletion = (from n in db.sifUserTasks
                                           where n.idTag == getTag.idTag && n.idTask == idTask
                                           select n).Single();
                deleteList.Add(forDeletion);
            }
            db.sifUserTasks.DeleteAllOnSubmit(deleteList);
            db.SubmitChanges();
            return Redirect("Index");
        }
    }
}
