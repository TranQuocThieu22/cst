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

        public TFSAccountController(ITFSAccountService tfsAccountService)
        {
            _tfsAccountService = tfsAccountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var winAccount = await _tfsAccountService.LoginAsync(model);
                // Here you might generate a JWT token or set up a session
                return Ok(new { message = "Login successful", user = winAccount });
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
