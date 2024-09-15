using Microsoft.AspNetCore.Mvc;

namespace BattleShipAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BoardController : ControllerBase
    {
        [HttpGet]
        [Route("hello")]
        public async Task<IActionResult> Get()
        {
            return Ok("Hello");
        }
    }
}
