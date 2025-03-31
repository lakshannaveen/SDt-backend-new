using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdt_backend.net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sdt_backend.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageStationDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManageStationDataController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/ManageStationData
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Station>>> GetStations()
        {
            return await _context.AirQualityStations
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/ManageStationData/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Station>> GetStation(int id)
        {
            var station = await _context.AirQualityStations
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return station == null ? NotFound() : station;
        }

        // POST: api/ManageStationData
        [HttpPost]
        public async Task<ActionResult<Station>> PostStation(Station station)
        {
            var now = DateTime.UtcNow;
            station.Timestamp = now;
            station.CreatedAt = now;
            station.UpdatedAt = now;

            _context.AirQualityStations.Add(station);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStation), new { id = station.Id }, station);
        }

        // PUT: api/ManageStationData/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutStation(int id, Station station)
        {
            if (id != station.Id)
            {
                return BadRequest("ID mismatch");
            }

            station.UpdatedAt = DateTime.UtcNow;
            _context.Entry(station).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!StationExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/ManageStationData/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            var station = await _context.AirQualityStations.FindAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            _context.AirQualityStations.Remove(station);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StationExists(int id)
        {
            return _context.AirQualityStations.Any(e => e.Id == id);
        }
    }
}