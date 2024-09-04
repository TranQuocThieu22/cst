using educlient.Data;
using educlient.Models;
using educlient.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
                var winAccount = await _tfsAccountService.LoginAsync(model);
                // Here you might generate a JWT token or set up a session
                return Ok(new { message = "Login successful", user = winAccount });

                if (model.username.ToLower() == "admin" && model.password == "12345")
                {
                    var data = new DsThongTinCaNhanDataDO()
                    {
                        id = Guid.NewGuid(),
                        TFSName = "admin",
                        fullName = "admin",
                        email = "admin",
                        phone = "admin",
                        avatar = null,
                        birthDate = new DateTime(2024, 1, 1),
                        startDate = new DateTime(2024, 1, 1),
                        nickName = "admin",
                        role = "admin",
                        isLeader = true,
                        isLunch = true,
                        WFHQuota = 1,
                        absenceQuota = 1,
                        isActive = true
                    };

                    return Ok(data);
                }

                else if (model.password != null && model.password == "1234")
                {
                    var AQMemberTable = database.Table<AQMember>();
                    var data = AQMemberTable.FindOne(x => x.TFSName == model.username);

                    var aqMemberReturn = new AQMemberInput
                    {
                        id = data.id,
                        tfsName = data.TFSName,
                        fullName = data.fullName,
                        email = data.email,
                        phone = data.phone,
                        avatar = data.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(data.avatar)}" : null,
                        birthDate = data.birthDate,
                        startDate = data.startDate,
                        nickName = data.nickName,
                        role = data.role,
                        isLeader = data.isLeader,
                        isLunch = data.isLunch,
                        WFHQuota = data.WFHQuota,
                        absenceQuota = data.absenceQuota,
                        isActive = data.isActive
                    };

                    return Ok(aqMemberReturn);
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
    }
}
