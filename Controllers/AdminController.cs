using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdt_backend.net.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace sdt_backend.net.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterDto adminDto)
        {
            try
            {
                // Check if username already exists
                if (await _context.Admins.AnyAsync(a => a.Username.ToLower() == adminDto.Username.ToLower()))
                {
                    return BadRequest(new { message = "Username already exists!" });
                }

                // Create and save admin
                var admin = new Admin
                {
                    Username = adminDto.Username,
                    PasswordHash = HashPassword(adminDto.Password)
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin registered successfully!" });
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Database Error: {dbEx.Message}");
                return StatusCode(500, new { message = "A database error occurred. Please try again later." });
            }
            catch (Exception ex)
            {
                // Handle general errors
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAdmins()
        {
            try
            {
                var admins = await _context.Admins
                    .Select(a => new { a.Id, a.Username })
                    .ToListAsync();

                return Ok(admins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound(new { message = "Admin not found." });
                }

                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin deleted successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while deleting the admin." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto adminDto)
        {
            try
            {
                // Check if the admin exists
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username.ToLower() == adminDto.Username.ToLower());
                if (admin == null)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // Validate the password
                var hashedPassword = HashPassword(adminDto.Password);
                if (admin.PasswordHash != hashedPassword)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                return Ok(new { message = "Login successful!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }

    public class AdminRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AdminLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}