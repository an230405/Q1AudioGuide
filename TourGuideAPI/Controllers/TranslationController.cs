using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public TranslationController(AudioGuideDbContext context) => _context = context;

        // GET api/Translation
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Translations
                .Select(t => new
                {
                    t.Id, t.PoiId,
                    PoiName = t.Poi != null ? t.Poi.Name : null,
                    t.LanguageId,
                    LanguageName = t.Language != null ? t.Language.Name : null,
                    t.Title, t.Content, t.CreatedAt
                })
                .ToListAsync();
            return Ok(data);
        }

        // GET api/Translation/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _context.Translations
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id, t.PoiId,
                    PoiName = t.Poi != null ? t.Poi.Name : null,
                    t.LanguageId,
                    LanguageName = t.Language != null ? t.Language.Name : null,
                    t.Title, t.Content, t.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (t == null) return NotFound();
            return Ok(t);
        }

        // POST api/Translation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Translation translation)
        {
            translation.CreatedAt = DateTime.Now;
            _context.Translations.Add(translation);
            await _context.SaveChangesAsync();
            return Ok(new { translation.Id });
        }

        // PUT api/Translation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Translation translation)
        {
            if (id != translation.Id) return BadRequest();
            _context.Entry(translation).State = EntityState.Modified;
            _context.Entry(translation).Property(x => x.CreatedAt).IsModified = false;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Translations.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/Translation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.Translations.FindAsync(id);
            if (t == null) return NotFound();
            _context.Translations.Remove(t);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
