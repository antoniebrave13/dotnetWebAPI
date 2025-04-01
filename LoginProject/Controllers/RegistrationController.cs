using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using LoginProject.Models;
using System.Xml.Linq;
using System.Data.Common;
using LoginProject.Helpers;
using System.Web;
using System.IO;
using System.Threading.Tasks;

namespace LoginProject.Controllers
{
    [RoutePrefix("api/Users")]
    public class RegistrationController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
        SqlCommand cmd = null;

        public byte[] ConvertImageToByteArray(HttpPostedFileBase file)
        {
            byte[] imageBytes = null;

            using (BinaryReader reader = new BinaryReader(file.InputStream))
            {
                imageBytes = reader.ReadBytes(file.ContentLength);
            }

            return imageBytes;
        }

        [HttpPost]
        [Route("Registration")]
        public async Task<HttpResponseMessage> Registration()
        {
            string msg = string.Empty;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                }

                var provider = new MultipartFormDataStreamProvider(HttpContext.Current.Server.MapPath("~/App_Data"));

                await Request.Content.ReadAsMultipartAsync(provider);

                var employee = new Employee
                {
                    Username = provider.FormData["Username"],
                    Password = provider.FormData["Password"],
                    Status = provider.FormData["Status"],
                    Firstname = provider.FormData["Firstname"],
                    Lastname = provider.FormData["Lastname"],
                    Email = provider.FormData["Email"],
                    Expiry = DateTime.Now.AddYears(1)
                };

                byte[] imageBytes = null;

                // Handle image upload
                var fileData = provider.FileData.FirstOrDefault();
                if (fileData != null)
                {
                    var filePath = fileData.LocalFileName;
                    using (var binaryReader = new BinaryReader(File.OpenRead(filePath)))
                    {
                        imageBytes = binaryReader.ReadBytes((int)new FileInfo(filePath).Length);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Signature image is required.");
                }

                employee.Signature = imageBytes;

                cmd = new SqlCommand("usp_Registration", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", employee.Username);
                cmd.Parameters.AddWithValue("@expiry", employee.Expiry);
                cmd.Parameters.AddWithValue("@status", employee.Status);

                // Encrypt the password before saving
                string encryptedPassword = EncryptionHelper.EncryptString(employee.Password);
                cmd.Parameters.AddWithValue("@password", encryptedPassword);
                cmd.Parameters.AddWithValue("@firstname", employee.Firstname);
                cmd.Parameters.AddWithValue("@lastname", employee.Lastname);
                cmd.Parameters.AddWithValue("@email", employee.Email);

                // Add the signature as a binary parameter
                cmd.Parameters.AddWithValue("@signature", employee.Signature);

                await conn.OpenAsync();
                int i = await cmd.ExecuteNonQueryAsync();
                conn.Close();

                if (i > 0)
                {
                    msg = "Record has been inserted successfully";
                }
                else
                {
                    msg = "An error occurred.";
                }

                return Request.CreateResponse(HttpStatusCode.OK, msg);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}

