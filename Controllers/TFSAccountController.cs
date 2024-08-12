using educlient.Data;
using educlient.Models;
using educlient.Services;
using Microsoft.AspNetCore.Components.Forms;
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
                //var winAccount = await _tfsAccountService.LoginAsync(model);
                // Here you might generate a JWT token or set up a session
                //return Ok(new { message = "Login successful", user = winAccount });

                if (model.password != null && model.password == "1234")
                {
                    var tb = database.Table<DsThongTinCaNhanDataDO>();
                    var data = tb.FindOne(x => x.TFSName == model.username);

                    return Ok(data);
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
