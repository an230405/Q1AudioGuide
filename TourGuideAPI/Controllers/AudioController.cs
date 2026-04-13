using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideAPI.Models;

namespace TourGuideAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly AudioGuideDbContext _context;
        public AudioController(AudioGuideDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Audios
                .Select(a => new
                {
                    a.Id, a.PoiId,
                    PoiName = a.Poi != null ? a.Poi.Name : null,
                    a.LanguageId,
                    LanguageName = a.Language != null ? a.Language.Name : null,
                    a.AudioUrl, a.Duration, a.CreatedAt
                })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var a = await _context.Audios
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id, a.PoiId,
                    PoiName = a.Poi != null ? a.Poi.Name : null,
                    a.LanguageId,
                    LanguageName = a.Language != null ? a.Language.Name : null,
                    a.AudioUrl, a.Duration, a.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (a == null) return NotFound();
            return Ok(a);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Audio audio)
        {
            audio.CreatedAt = DateTime.Now;
            _context.Audios.Add(audio);
            await _context.SaveChangesAsync();
            return Ok(new { audio.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Audio audio)
        {
            if (id != audio.Id) return BadRequest();
            _context.Entry(audio).State = EntityState.Modified;
            _context.Entry(audio).Property(x => x.CreatedAt).IsModified = false;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Audios.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();
            _context.Audios.Remove(audio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
