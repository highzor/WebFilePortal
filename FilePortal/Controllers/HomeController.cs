
using FilePortal.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FilePortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public async System.Threading.Tasks.Task<ActionResult> UserPage()
        {
            
            Guid userGuid = new Guid(User.Identity.GetUserId());
            WebRequest request = WebRequest.Create("https://localhost:44385/Home/GetFileList?UserId="+userGuid);
            WebResponse response = await request.GetResponseAsync();
            string result;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                   result = reader.ReadToEnd();
                }
            }
            var doc = JsonConvert.DeserializeObject<List<DocumentDTO>>(result);
            response.Close();

            ViewBag.Files = doc;
            return View();
        }
        public ViewResult UserFilesList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Upload()
        {
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    
                    DocumentDTO doc = new DocumentDTO();
                    string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    //upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                    MemoryStream target = new MemoryStream();
                    upload.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();
                    doc.Content = data;
                    doc.CreateDate = DateTime.Now;
                    doc.FileName = fileName;
                    doc.FileId = Guid.NewGuid();
                    doc.MimeType = upload.ContentType;
                    doc.UserName = User.Identity.GetUserName();
                    doc.UserId = new Guid(User.Identity.GetUserId());
                    doc.FileSize = Decimal.Divide(doc.Content.Length, 1048576);
                    //doc.FileSize = doc.Content.Length / 1048576;
                    doc.FileNameInFileStorage = doc.FileId.ToString();
                    doc.Description = "example";
                    // client.InsertFile(doc);
                    string json = JsonConvert.SerializeObject(doc);
                    var httpRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44385/Home/InsertFile");
                    httpRequest.Method = "POST";
                    httpRequest.ContentType = "application/json";
                    using (var requestStream = httpRequest.GetRequestStream())
                    using (var writer = new StreamWriter(requestStream))
                    {
                        writer.Write(json);
                    }
                    using (var httpResponse = httpRequest.GetResponse());
                }
            }
            return Json("файл загружен");
        }

        public async System.Threading.Tasks.Task<JsonResult> Delete(string[] id)
        {
            if (id.Length > 0)
            {
                foreach (string onesId in id)
                {
                    Guid fileId = new Guid(onesId);

                    WebRequest request = WebRequest.Create("https://localhost:44385/Home/DeleteFile?Id=" + fileId);
                    WebResponse response = await request.GetResponseAsync();
                }
                return Json("файлы удалены");
            }
            return Json("файлы не удалены");
        }
        public async System.Threading.Tasks.Task<ActionResult> Download(Guid id)
        {
            WebRequest request = WebRequest.Create("https://localhost:44385/Home/GetFile?Id=" + id);
            WebResponse response = await request.GetResponseAsync();
            string result;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            string trans = JsonConvert.DeserializeObject<string>(result);
            DocumentDTO document = JsonConvert.DeserializeObject<DocumentDTO>(trans);

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + document.FileName);
            Response.AddHeader("Content-Length", document.Content.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.OutputStream.Write(document.Content, 0, document.Content.Length);
            Response.End();
            return RedirectToRoute(new { controller = "Home", action = "UserPage" });
        }
    }
}