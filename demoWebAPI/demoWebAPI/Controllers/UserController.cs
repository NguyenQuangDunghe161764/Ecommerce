using demoWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly EcomDbContext _context;

        public UsersController(EcomDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Users.ToList());
        }
    }
}
