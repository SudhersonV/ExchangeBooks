using System.Threading.Tasks;
using ExchangeBooks.Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeBooks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // [HttpGet("[action]")]
        // public async Task<IActionResult> Clean()
        // {
        //     await _adminService.Clean();
        //     return NoContent();
        // }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create()
        {
            await _adminService.Create();
            return Ok();
        }

        // [HttpDelete("[action]")]
        // public async Task<IActionResult> Delete()
        // {
        //     await _adminService.Delete();
        //     return Ok();
        // }
    }
}