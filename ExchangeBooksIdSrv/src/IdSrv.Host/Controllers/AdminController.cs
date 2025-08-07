using System.Threading.Tasks;
using IdSrv.Cosmos.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdSrv.Host.Controllers
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
        //     return Ok();
        // }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create()
        // {
        //     await _adminService.Create();
        //     return Ok();
        // }

        // [HttpDelete("[action]")]
        // public async Task<IActionResult> Delete()
        // {
        //     await _adminService.Delete();
        //     return Ok();
        // }
    }
}