using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using UploadFile_MVC5.Models;

namespace UploadFile_MVC5.Controllers
{
    public class HomeController : Controller
    {
        public UploadDbContext _context;

        public HomeController()
        {
            _context = new UploadDbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadToDatabase()
        {
            return View();
        }
        public ActionResult LoadFiles()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult DisplayFilesFromDb(UploadDataViewModel model)
        {
            bool userExists = _context.Users.Where(x => x.UserName == model.UserName).Any();
            List<LoadFileViewModel> ufls = new List<LoadFileViewModel>();
            if (userExists)
            {
                var userId = _context.Users.Where(x => x.UserName == model.UserName).Select(x => x.Id).FirstOrDefault();
                var userFiles = _context.UserFiles.Where(x => x.UserId == userId).ToList();
                foreach (var userFile in userFiles)
                {
                    string type = null;
                    int index = userFile.FileType.IndexOf('/');
                    if (index > 0) { type = userFile.FileType.Substring(0, index); }
                    ufls.Add(new LoadFileViewModel() { FileName = userFile.FileName, FileType = type, Id = userFile.Id });
                }
                return View(ufls);
            }
            else
            {
                ModelState.AddModelError("", "User Not Exists.");
                return RedirectToAction("LoadFiles", "Home");
            }
        }

        public ActionResult Media(string id)
        {
            var userFile = _context.UserFiles.Where(x => x.Id == id).FirstOrDefault();

            long fSize = userFile.UserFile.Length;
            long startbyte = 0;
            long endbyte = fSize - 1;
            int statusCode = 200;
            if ((Request.Headers["Range"] != null))
            {
                //Get the actual byte range from the range header string, and set the starting byte.
                string[] range = Request.Headers["Range"].Split(new char[] { '=', '-' });
                startbyte = Convert.ToInt64(range[1]);
                if (range.Length > 2 && range[2] != "") endbyte = Convert.ToInt64(range[2]);
                //If the start byte is not equal to zero, that means the user is requesting partial content.
                if (startbyte != 0 || endbyte != fSize - 1 || range.Length > 2 && range[2] == "")
                { statusCode = 206; }//Set the status code of the response to 206 (Partial Content) and add a content range header.                                    
            }
            long desSize = endbyte - startbyte + 1;
            //Headers
            Response.StatusCode = statusCode;
            Response.AddHeader("Content-Accept", userFile.FileType);
            Response.AddHeader("Content-Length", desSize.ToString());
            Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));

            var fs = new MemoryStream(userFile.UserFile, (int)startbyte, (int)desSize);
            return new FileStreamResult(fs, userFile.FileType);
        }

        [HttpPost]
        public ActionResult UploadToDb(UploadDataViewModel model)
        {
            bool userExists = _context.Users.Where(x => x.UserName == model.UserName).Any();
            if (!userExists)
            {
                User us = new User { Id = Guid.NewGuid().ToString(), UserName = model.UserName };
                _context.Users.Add(us);
                _context.SaveChanges();
            }
            if (model.Files != null && model.Files.Count > 0 && model.Files[0] != null)
            {
                foreach (var file in model.Files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    bool fileExists = _context.UserFiles.Where(x => x.FileName == fileName).Any();
                    if (!fileExists)
                    {
                        var fileType = file.ContentType;
                        var fileContent = new byte[file.InputStream.Length];
                        var userId = _context.Users.Where(x => x.UserName == model.UserName).Select(x => x.Id).FirstOrDefault();
                        file.InputStream.Read(fileContent, 0, fileContent.Length);
                        UserFiles uf = new UserFiles
                        {
                            Id = Guid.NewGuid().ToString(),
                            FileName = fileName,
                            FileType = fileType,
                            UserFile = fileContent,
                            UserId = userId
                        };
                        _context.UserFiles.Add(uf);
                        _context.SaveChanges();
                        ModelState.AddModelError("", "File Uploaded.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "File exists.");
                    }
                    return RedirectToAction("UploadToDatabase");
                }
            }
            return RedirectToAction("UploadToDatabase");
        }




        [HttpPost]
        public ActionResult UploadToFolder(UploadViewModel model)
        {
            if (model.Files != null && model.Files.Count > 0 && model.Files[0] != null)
            {
                foreach (var file in model.Files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/files/"));
                    if (!directory.Exists) { directory.Create(); }
                    var filePath = HostingEnvironment.MapPath("~/Content/files/") + fileName;
                    file.SaveAs(filePath);
                }
                ModelState.AddModelError("", "File Uploaded.");
            }
            return RedirectToAction("Index");
        }

        public ActionResult DisplayFilesFromFolder()
        {
            Dictionary<string, string> pathAndExt = new Dictionary<string, string>();

            var filePaths = Directory.GetFiles(Server.MapPath("~/Content/files"));
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var fileExt = Path.GetExtension(filePath);
                var fileType = FileTypes[fileExt];
                pathAndExt.Add(fileName, fileType);
            }
            return View(pathAndExt);
        }

        Dictionary<string, string> FileTypes = new Dictionary<string, string>()
        {
            {".mp4","video" }, {".ogg","video" }, {".webm","video" }, {".mkv", "video" }, {".mpeg","video" }, {".mpg","video" }, {".ogv","video" }, {".ogx","video" }, {".3gp","video" }, {".3g2","video" }, {".m4v","video" }, {".jpe","image"}, {".jpg","image"}, {".gif","image"}, {".png","image"}, {".bmp","image"}, {".mp3","audio"}, {".oga","audio"}, {".m4a","audio"}, {".m4b","audio"}, {".m4r","audio"}, {".m3u","audio"}, {".pls","audio"}, {".opus","audio"}, {".amr","audio"}, {".wav","audio"}, {".lcka","audio"},
        };
    }
}
