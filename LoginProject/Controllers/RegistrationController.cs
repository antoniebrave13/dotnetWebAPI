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

namespace LoginProject.Controllers
{
    [RoutePrefix("api/Users")]
    public class RegistrationController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
        SqlCommand cmd = null;

        [HttpPost]
        [Route("Registration")]
        public string Registration(Employee employee)
        {
            string msg = string.Empty;
            try
            {
                // Set the expiry date to one year from today's date
                employee.Expiry = DateTime.Now.AddYears(1);

                cmd = new SqlCommand("usp_Registration", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", employee.Username);
                cmd.Parameters.AddWithValue("@expiry", employee.Expiry);
                cmd.Parameters.AddWithValue("@status", employee.Status);
                string encryptedPassword = EncryptionHelper.EncryptString(employee.Password);
                cmd.Parameters.AddWithValue("@password", encryptedPassword);
                cmd.Parameters.AddWithValue("@firstname", employee.Firstname);
                cmd.Parameters.AddWithValue("@lastname", employee.Lastname);
                cmd.Parameters.AddWithValue("@email", employee.Email);

                conn.Open();
                int i = cmd.ExecuteNonQuery();
                conn.Close();

                if (i > 0)
                {
                    msg = "Record has been inserted";
                }
                else
                {
                    msg = "An error occurred.";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return msg;
        }

    }
}
