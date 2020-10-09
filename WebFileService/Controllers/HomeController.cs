using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebFileService.Models;

namespace WebFileService.Controllers
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

        public JsonResult InsertFile(DocumentDTO document)
        {
            string result = "";
            try
            {
                using (DataBaseHelper dbh = new DataBaseHelper())
                {
                    result = dbh.AddFileToDB(document);
                    return Json(result);
                }
            }
            catch
            {
                return Json($"Произошла ошибка при добавлении файла. Exception: {result}");
            }
        }
        public JsonResult GetFileList(Guid UserId)
        {
            try
            {
                using (DataBaseHelper dbh = new DataBaseHelper())
                {
                    var list = dbh.GetFileListFromDB(UserId);
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json($"Произошла ошибка при получении файлов. Exception: {ex.Message}");
            }
        }

        public JsonResult DeleteFile(Guid[] Id)
        {
            string result = "";
            try
            {
                foreach (var Ids in Id)
                {
                    using (DataBaseHelper dbh = new DataBaseHelper())
                    {
                        result = dbh.DeleteFileFromDB(Ids);
                    }
                }
                return Json(result);
            }
            catch
            {
                return Json($"Произошла ошибка при удалении файлов. Exception: {result}");
            }
        }
        public JsonResult GetFile(Guid Id)
        {
            try
            {
                using (DataBaseHelper dbh = new DataBaseHelper())
                {
                    string json = dbh.GetFileFromDB(Id);
                    return Json(json, JsonRequestBehavior.AllowGet);
                }
            } catch (Exception ex)
            {
                return Json($"Произошла ошибка при получении файла. Exception: {ex.Message}");
            }
     
        }

    }
}