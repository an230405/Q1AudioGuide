using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public LanguageController(AudioGuideDbContext context) => _context = context;

        // GET api/Language
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Languages.ToListAsync());

        // GET api/Language/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var lang = await _context.Languages.FindAsync(id);
            if (lang == null) return NotFound();
            return Ok(lang);
        }

        // POST api/Language
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Language language)
        {
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = language.Id }, language);
        }

        // PUT api/Language/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Language language)
        {
            if (id != language.Id) return BadRequest();
            _context.Entry(language).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Languages.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lang = await _context.Languages.FindAsync(id);
            if (lang == null) return NotFound();
            _context.Languages.Remove(lang);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
