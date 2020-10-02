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

        public void InsertFile(DocumentDTO document)
        {
            Decimal FileSize = Decimal.Divide(document.Content.Length, 1048576);
            string fullNamePath = "";

            int size = 1000000;
            if (ConfigurationManager.AppSettings["fileSize"] != null)
            {
                size = Int32.Parse(ConfigurationManager.AppSettings["fileSize"]);
            }
            if (document.Content.Length > size)
            {
                fullNamePath = $"{ConfigurationManager.AppSettings["path"]}{document.FileNameInFileStorage}";
                using (FileStream fstream = new FileStream(fullNamePath, FileMode.OpenOrCreate))
                {
                    fstream.Write(document.Content, 0, document.Content.Length);
                }
                string connectionString = ConfigurationManager.ConnectionStrings["FileService"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO FileTable VALUES (@FileId, @UserId, @UserName, @FileName, @FileNameInFileStorage, @MimeType, @Description, @Content, @FileSize, @CreateDate)";
                    command.Parameters.AddWithValue("@FileId", document.FileId);
                    command.Parameters.AddWithValue("@UserId", document.UserId);
                    command.Parameters.AddWithValue("@UserName", document.UserName);
                    command.Parameters.AddWithValue("@FileName", document.FileName);
                    command.Parameters.AddWithValue("@FileNameInFileStorage", document.FileNameInFileStorage);
                    command.Parameters.AddWithValue("@MimeType", document.MimeType);
                    command.Parameters.AddWithValue("@Description", document.Description);
                    command.Parameters.Add("@Content", SqlDbType.VarBinary, -1);
                    command.Parameters["@Content"].Value = DBNull.Value;
                    command.Parameters.AddWithValue("@FileSize", document.FileSize = FileSize);
                    command.Parameters.AddWithValue("@CreateDate", document.CreateDate);
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                string connectionString = ConfigurationManager.ConnectionStrings["FileService"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO FileTable VALUES (@FileId, @UserId, @UserName, @FileName, @FileNameInFileStorage, @MimeType, @Description, @Content, @FileSize, @CreateDate)";
                    command.Parameters.AddWithValue("@FileId", document.FileId);
                    command.Parameters.AddWithValue("@UserId", document.UserId);
                    command.Parameters.AddWithValue("@UserName", document.UserName);
                    command.Parameters.AddWithValue("@FileName", document.FileName);
                    command.Parameters.AddWithValue("@FileNameInFileStorage", document.FileNameInFileStorage).Value = DBNull.Value;
                    command.Parameters.AddWithValue("@MimeType", document.MimeType);
                    command.Parameters.AddWithValue("@Description", document.Description);
                    command.Parameters.AddWithValue("@Content", document.Content);
                    command.Parameters.AddWithValue("@FileSize", document.FileSize = FileSize);
                    command.Parameters.AddWithValue("@CreateDate", document.CreateDate);
                    command.ExecuteNonQuery();
                }
            }
           // return Json("ok");
        }
        public JsonResult GetFileList(Guid UserId)
        {
            List<DocumentDTO> list = new List<DocumentDTO>();
            string connectionString = ConfigurationManager.ConnectionStrings["FileService"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"SELECT FileName, Description, FileSize, CreateDate, FileId FROM FileTable WHERE UserId=@UserId";
                SqlParameter guiParam = new SqlParameter("@UserId", UserId);
                command.Parameters.Add(guiParam);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DocumentDTO document = new DocumentDTO();
                        document.FileName = reader.GetValue(0).ToString();
                        document.Description = reader.GetValue(1).ToString();
                        document.FileSize = (Decimal)reader.GetValue(2);
                        document.CreateDate = (DateTime)reader.GetValue(3);
                        document.FileId = (Guid)reader.GetValue(4);
                        list.Add(document);
                    }
                }
                reader.Close();
            }
           
            var json = list;

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public void DeleteFile(Guid Id)
        {
            DocumentDTO document = new DocumentDTO();
            string connectionString = ConfigurationManager.ConnectionStrings["FileService"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"SELECT FileNameInFileStorage FROM FileTable WHERE FileId=@Id";
                SqlParameter guiParam = new SqlParameter("@Id", Id);
                command.Parameters.Add(guiParam);
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                document.FileNameInFileStorage = reader.GetValue(0).ToString();
            }

            if (document.FileNameInFileStorage.Equals(""))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"DELETE FROM FileTable WHERE FileId=@Id";
                    SqlParameter guiParam = new SqlParameter("@Id", Id);
                    command.Parameters.Add(guiParam);
                    command.ExecuteNonQuery();
                    //return Json("Ok");
                }
            }
            else
            {
                string fullNamePath = $"{ConfigurationManager.AppSettings["path"]}{document.FileNameInFileStorage}";
                FileInfo myFile = new FileInfo(fullNamePath);
                if (myFile.Exists)
                {
                    myFile.Delete();
                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"DELETE FROM FileTable WHERE FileId=@Id";
                    SqlParameter guiParam = new SqlParameter("@Id", Id);
                    command.Parameters.Add(guiParam);
                    command.ExecuteNonQuery();
                   // return Json("Ok");
                }
            }
            //return Json("file not found");
        }
        public JsonResult GetFile(Guid Id)
        {
            DocumentDTO document = new DocumentDTO();
            document.FileId = Id;
            string connectionString = ConfigurationManager.ConnectionStrings["FileService"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"SELECT FileNameInFileStorage, FileName, MimeType, Content FROM FileTable WHERE FileId=@Id";
                SqlParameter guiParam = new SqlParameter("@Id", document.FileId);
                command.Parameters.Add(guiParam);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        document.FileNameInFileStorage = reader.GetValue(0).ToString();
                        document.FileName = reader.GetValue(1).ToString();
                        document.MimeType = reader.GetValue(2).ToString();
                        try
                        {
                            document.Content = (byte[])reader.GetValue(3);
                        }
                        catch { }
                    }
                }
                reader.Close();
            }
            if (!document.FileNameInFileStorage.Equals(""))
            {
                string fullNamePath = $"{ConfigurationManager.AppSettings["path"]}{document.FileNameInFileStorage}";
                using (FileStream fstream = System.IO.File.OpenRead(fullNamePath))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    document.Content = array;
                }
            }
            string json = JsonConvert.SerializeObject(document);
            return Json(json, JsonRequestBehavior.AllowGet); ;
        }

    }
}