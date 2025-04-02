using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdt_backend.net.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace sdt_backend.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(AppDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostContact([FromBody] Contact contact)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid contact form submission");
                return BadRequest(new { message = "Invalid form data" });
            }

            try
            {
                contact.SubmittedAt = DateTime.UtcNow;
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New contact form submitted: {Email}", contact.Email);
                return Ok(new { message = "Contact form submitted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving contact form");
                return StatusCode(500, new { message = "Database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving contact form");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllContacts()
        {
            try
            {
                var contacts = await _context.Contacts
                    .OrderByDescending(c => c.SubmittedAt)
                    .ToListAsync();

                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contacts");
                return StatusCode(500, new { message = "Error fetching contacts" });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);
                if (contact == null)
                {
                    return NotFound(new { message = "Contact not found" });
                }

                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Contact deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact");
                return StatusCode(500, new { message = "Error deleting contact" });
            }
        }
    }
}