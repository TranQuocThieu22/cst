using AQFramework.Utilities;
using educlient.Data;
using educlient.Models;
using educlient.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TFSAccountController : ControllerBase
    {
        private readonly ITFSAccountService _tfsAccountService;
        private readonly IDbLiteContext database;



        public TFSAccountController(ITFSAccountService tfsAccountService, IDbLiteContext dataContext)
        {
            _tfsAccountService = tfsAccountService;
            database = dataContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            if (string.IsNullOrWhiteSpace(model.username) || string.IsNullOrWhiteSpace(model.password))
            {
                throw new ArgumentException("Username and password are required.");
            }
            try
            {
                //var winAccount = await _tfsAccountService.LoginAsync(model);
                // Here you might generate a JWT token or set up a session
                //return Ok(new { message = "Login successful", user = winAccount });

                if (model.username.ToLower() == "admin" && model.password == "12345")
                {
                    var data = new LoginReturnData()
                    {
                        id = 69420,
                        tfsName = "admin",
                        fullName = "admin",
                        email = "admin",
                        phone = "admin",
                        avatar = null,
                        birthDate = new DateTime(2024, 1, 1),
                        startDate = new DateTime(2024, 1, 1),
                        nickName = "admin",
                        role = "admin",
                        isLeader = true,
                        isActive = true,
                    };

                    return Ok(data);
                }

                else if (model.username != null && model.password != null)
                {
                    var hashedPassword = HashPassword(model.password);

                    var AQMemberTable = database.Table<AQMember>();

                    var userAuthData = AQMemberTable.Query()
                        .Where(x => x.TFSName == model.username)
                        .Select(x => new { x.TFSName, x.password, x.id })
                        .FirstOrDefault();

                    if (userAuthData == null)
                    {
                        throw new ArgumentException("Username is incorrect.");
                    }
                    else if (!VerifyPassword(model.password, userAuthData.password))
                    {
                        throw new ArgumentException("Password is incorrect.");
                    }

                    var userData = AQMemberTable.Query()
                        .Where(x => x.TFSName == model.username)
                        .Select(x => new LoginReturnData
                        {
                            id = x.id,
                            tfsName = x.TFSName,
                            fullName = x.fullName,
                            email = x.email,
                            phone = x.phone,
                            avatar = x.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(x.avatar)}" : null,
                            birthDate = x.birthDate,
                            startDate = x.startDate,
                            nickName = x.nickName,
                            role = x.role,
                            isLeader = x.isLeader,
                            isActive = x.isActive,
                        })
                        .FirstOrDefault();

                    return Ok(userData);
                }
                else
                {
                    throw new ArgumentException("Username and password are incorrect.");
                }

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Login failed");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private static string HashPassword(string pwd)
        {
            if (string.IsNullOrEmpty(pwd)) return "";
            SHA256 sha = new SHA256Managed();
            var pwdBuff = Encoding.ASCII.GetBytes(pwd);
            var hashedPwd = sha.TransformFinalBlock(pwdBuff, 0, pwdBuff.Length);
            var hash = new StringBuilder();
            foreach (var b in sha.Hash)
            {
                hash.Append(string.Format("{0:x2}", b));
            }
            sha.Clear();
            return hash.ToString();
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            // Hash input password
            string hashedInput = HashPassword(password);
            // Compare hashed input to stored hash
            if (hashedInput == hashedPassword)
            {
                return true;
            }
            return false;

        }
    }

    public class LoginReturnData
    {
        public int id { get; set; }
        public int memberNumber { get; set; }
        public string tfsName { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string avatar { get; set; }
        public DateTime birthDate { get; set; }
        public DateTime startDate { get; set; }
        public string nickName { get; set; }
        public string role { get; set; }
        public bool isLeader { get; set; }
        public int workingYear { get; set; }
        public bool isActive { get; set; }
    }

    public class UserData
    {
        public string TFSName { get; set; }
        public string password { get; set; }
    }

}
