using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mash_v0._3.Models;
using System.Web.Security;
using System.IO;

namespace Mash_v0._3.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        databaseDataContext db = new databaseDataContext();

        public ViewResult Files()
        {
            string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
            User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

            Project activeProject = Session["Project"] as Project;
            
            if (TempData["curPage"] == null)
            {
                TempData["curPage"] = 0;
            }

            var getFiles = from n in db.Files
                           join s in db.sif_ProjectFiles on n.idFile equals s.idFile
                           where s.idProject == activeProject.idProject
                           select n;
            if (getFiles.Count() != 0)
            {
                ViewData["files"] = getFiles.ToList();
            }
            else
            {
                ViewData["files"] = null;
            }

            var getTags = from n in db.sif_TagFiles
                          join s in db.Tags on n.idTag equals s.idTag
                          join a in db.Files on n.idFile equals a.idFile
                          join b in db.sif_ProjectFiles on a.idFile equals b.idFile
                          where b.idProject == activeProject.idProject
                          select s;

            if (getTags.Count() != 0)
            {
                ViewData["tags"] = getTags.ToList();
            }
            else
            {
                ViewData["tags"] = null;
            }

            var getSifTags = from n in db.sif_TagFiles
                             join s in db.Tags on n.idTag equals s.idTag
                             join a in db.Files on n.idFile equals a.idFile
                             join b in db.sif_ProjectFiles on a.idFile equals b.idFile
                             where b.idProject == activeProject.idProject
                             select n;

            if (getSifTags.Count() != 0)
            {
                ViewData["sifTags"] = getSifTags.ToList();
            }
            else
            {
                ViewData["sifTags"] = null;
            }

            return View();
        }

        public RedirectResult uploadFile(string tagList)
        {
            foreach (string inputTagName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[inputTagName];
                if (file.ContentLength > 0)
                {
                    string filePath = Server.MapPath("") + "\\" + file.FileName;
                    file.SaveAs(filePath);

                    Mash_v0._3.Models.File newFile = new Mash_v0._3.Models.File();

                    string email = ((FormsIdentity)System.Web.HttpContext.Current.User.Identity).Name.ToString();
                    User signedUser = db.Users.Where(user => user.email.Trim() == email).Single();

                    Project activeProject = Session["project"] as Project;

                    newFile.fileName = file.FileName;
                    newFile.idOwner = signedUser.idUser;
                    newFile.path = "Files\\" + file.FileName;
                    newFile.description = file.ContentType;
                    newFile.dateCreated = DateTime.Now;
                    newFile.dateUpdated = DateTime.Now;

                    db.Files.InsertOnSubmit(newFile);
                    db.SubmitChanges();

                    sif_ProjectFile newEntry = new sif_ProjectFile();
                    newEntry.idFile = newFile.idFile;
                    newEntry.idProject = activeProject.idProject;
                    db.sif_ProjectFiles.InsertOnSubmit(newEntry);
                    db.SubmitChanges();
                }
            }

            return Redirect("Files");
        }

        public RedirectResult Download(string path)
        {
            string filename = path;
            FileInfo fileInfo = new FileInfo(filename);

            if (fileInfo.Exists)
            {
                Response.Clear();
                Response.AddHeader("Content-Disposition", "inline;attachment; filename=" + fileInfo.Name);
                Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                Response.ContentType = "application/octet-stream";
                Response.Flush();
                Response.WriteFile(fileInfo.FullName);
                Response.End();
            }

            return Redirect("Files");
        }

        public PartialViewResult ShowTagControl(string idFile)
        {
            ViewData["idFile"] = idFile;
            return PartialView();
        }

        [HttpPost]
        public RedirectResult AddTags(string tags, string fileID)
        {
            string[] tag = tags.Split(' ');


            foreach (var item in tag)
            {
                Tag newTag = new Tag();

                var getTag = from n in db.Tags
                             where n.tagName.Trim() == item
                             select n;
                if (getTag.Count() == 0)
                {
                    newTag.tagName = item;
                    db.Tags.InsertOnSubmit(newTag);
                    db.SubmitChanges();

                    sif_TagFile newTagFile = new sif_TagFile();
                    newTagFile.idFile = Int32.Parse(fileID);
                    newTagFile.idTag = newTag.idTag;
                    db.sif_TagFiles.InsertOnSubmit(newTagFile);
                    db.SubmitChanges();
                }
                else
                {
                    sif_TagFile newTagFile = new sif_TagFile();
                    newTagFile.idFile = Int32.Parse(fileID);
                    newTagFile.idTag = getTag.First().idTag;
                    db.sif_TagFiles.InsertOnSubmit(newTagFile);
                    db.SubmitChanges();
                }
            }
            return Redirect("Files");
        }

        public PartialViewResult ShowTaggedFiles(string idTag)
        {
            int tagID = Int32.Parse(idTag);
            Project activeProject = Session["Project"] as Project;

            var getFilesByTag = from n in db.Tags
                                join s in db.sif_TagFiles on n.idTag equals s.idTag
                                join a in db.sif_ProjectFiles on s.idFile equals a.idFile
                                join b in db.Files on a.idFile equals b.idFile
                                where n.idTag == tagID && a.idProject == activeProject.idProject
                                select b;
            if (getFilesByTag.Count() != 0)
            {
                ViewData["FilesByTag"] = getFilesByTag.ToList();
            }
            return PartialView();
        }


        public PartialViewResult ConfirmDeletion(string fileID)
        {
            ViewData["fileid"] = fileID;
            return PartialView();
        }

        [HttpPost]
        public RedirectResult DeleteFile(string fileID)
        {
            int id = Int32.Parse(fileID);

            var file = (from n in db.Files
                        where n.idFile == id
                        select n).Single();

            var fileSifrarnik = from n in db.sif_ProjectFiles
                                where n.idFile == id
                                select n;

            var fileTag = from n in db.sif_TagFiles
                          where n.idFile == id
                          select n;

            db.sif_ProjectFiles.DeleteAllOnSubmit(fileSifrarnik);
            db.sif_TagFiles.DeleteAllOnSubmit(fileTag);
            db.Files.DeleteOnSubmit(file);
            db.SubmitChanges();

            return Redirect("Files");
        }

        public PartialViewResult ShowUploadControls()
        {
            return PartialView();
        }

        public RedirectResult PreviousPage(int currectPage)
        {
            TempData["curPage"] = currectPage - 1;
            if (currectPage - 1 < 0)
                TempData["curPage"] = 0;
            return Redirect("Files");
        }

        public RedirectResult NextPage(int currectPage)
        {
            TempData["curPage"] = currectPage + 1;

            return Redirect("Files");
        }

        public RedirectResult SelectPage(int selPage)
        {
            TempData["curPage"] = selPage;
            return Redirect("Files");
        }

    }
}
