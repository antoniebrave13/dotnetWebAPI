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
                            Signature = row["signature"] != DBNull.Value ? (byte[])row["signature"] : null // Retrieve the signature image
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

        [HttpGet]
        [Route("GetSignature/{userID}")]
        public HttpResponseMessage GetSignature(int userID)
        {
            try
            {
                byte[] signatureData = null;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT signature FROM tblUserProfile WHERE userID = @userID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        var result = cmd.ExecuteScalar();

                        // Check if the signature data exists in the database
                        if (result != DBNull.Value)
                        {
                            // Convert hexadecimal string to byte array if needed
                            string hexData = result.ToString();
                            if (hexData.StartsWith("0x"))
                            {
                                signatureData = ConvertHexStringToByteArray(hexData.Substring(2));
                            }
                            else
                            {
                                signatureData = (byte[])result;
                            }
                        }
                    }
                }

                // If no signature data found, return 404 Not Found
                if (signatureData == null || signatureData.Length == 0)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Signature not found for this user.")
                    };
                }

                // You may choose to detect the image format based on the first few bytes (signatureData)
                // For simplicity, assuming it's PNG; you can adjust content type based on actual data.

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(signatureData)
                };

                // Assuming PNG image, adjust according to the actual type of the image
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png"); // Change to image/jpeg or image/gif if necessary

                return response;
            }
            catch (Exception ex)
            {
                // Return internal server error on failure
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error occurred: " + ex.Message)
                };
            }
        }

        // Helper method to convert hex string to byte array
        private byte[] ConvertHexStringToByteArray(string hex)
        {
            byte[] byteArray = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return byteArray;
        }


    }
}
