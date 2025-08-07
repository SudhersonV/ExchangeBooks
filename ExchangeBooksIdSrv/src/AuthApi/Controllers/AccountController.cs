using System.Threading.Tasks;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Authorize("checkwritescope")]
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ICosmosUserManager<ApplicationUser> _cosmosUserManager;
        private readonly IClaimsAccessor _claimsAccessor;
        public AccountController(ICosmosUserManager<ApplicationUser> cosmosUserManager
        , IClaimsAccessor claimsAccessor)
        {
            _cosmosUserManager = cosmosUserManager;
            _claimsAccessor = claimsAccessor;
        }

        
        [HttpPatch("[action]")]
        public async Task<IActionResult> Eula()
        {
            var email = _claimsAccessor.Email;
            var idp = _claimsAccessor.Idp;
            await _cosmosUserManager.AcceptTermsOfUse(email, idp);
            return Ok();
        }
    }
}