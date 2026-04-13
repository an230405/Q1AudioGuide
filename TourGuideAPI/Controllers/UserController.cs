using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public UserController(AudioGuideDbContext context) => _context = context;

        // GET api/User
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Không trả về Password
            var data = await _context.Users
                .Select(u => new { u.Id, u.Username, u.Role, u.CreatedAt })
                .ToListAsync();
            return Ok(data);
        }

        // GET api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Username, user.Role, user.CreatedAt });
        }

        // POST api/User
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            user.CreatedAt = DateTime.Now;
            // Trong thực tế nên hash password ở đây
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = user.Id },
                new { user.Id, user.Username, user.Role, user.CreatedAt });
        }

        // PUT api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            if (id != user.Id) return BadRequest();
            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Username = user.Username;
            existing.Role = user.Role;
            if (!string.IsNullOrEmpty(user.Password))
                existing.Password = user.Password; // hash trong thực tế
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
