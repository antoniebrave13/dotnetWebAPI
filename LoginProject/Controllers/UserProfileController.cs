using LoginProject.Helpers;
using LoginProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

namespace LoginProject.Controllers
{
    [RoutePrefix("api/Users")]
    public class UserProfileController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
        SqlDataAdapter dataAdapter = null;

        [HttpPost]
        [Route("Profile")]
        public IHttpActionResult Profile(UserInfo userInfo)
        {
            try
            {
                if (userInfo.userID == 0)
                {
                    return BadRequest("User ID cannot be zero.");
                }

                dataAdapter = new SqlDataAdapter("usp_UserProfile", conn);
                dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                dataAdapter.SelectCommand.Parameters.AddWithValue("@userID", userInfo.userID);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    var userList = new List<UserInfo>();
                    foreach (DataRow row in dt.Rows)
                    {
                        var user = new UserInfo
                        {
                            userID = Convert.ToInt32(row["userID"]),
                            Username = row["username"].ToString(),
                            Status = row["status"].ToString(),
                            Firstname = row["first_name"].ToString(),
                            Lastname = row["last_name"].ToString(),
                            Email = row["email"].ToString(),
                        };
                        userList.Add(user);
                    }
                    return Ok(userList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
