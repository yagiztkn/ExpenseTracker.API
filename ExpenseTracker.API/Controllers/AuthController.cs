using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.API.DTOs;
using ExpenseTracker.API.Models;    
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ExpenseTracker.API.Data;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            CreatePassordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı başarıyla kaydedildi.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return BadRequest("Kullanıcı bulunamadı.");
            }
            if (!VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Hatalı şifre.");
            }

            string token = CreateToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); 

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Token = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Geçersiz veya süresi dolmuş yenileme anahtarı. Lütfen tekrar giriş yapın.");
            }
            var newAcsessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
           
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Token = newAcsessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> Revoke(RefreshTokenDto dto)
        {
            var UserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserIdString))
            {
                return BadRequest("Kullanıcı kimliği bulunamadı."); 
            }

            var userId = int.Parse(UserIdString);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Kullanıcı bulunamadı.");
            }
            
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Çıkış işlemi başarılı, güvenlik anahtarları imha edildi.");
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Geçersiz Kullanıcı Token'ı.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı Bulunamadı.");
            }

            user.Username = dto.UpdatedName;
            user.Email = dto.UpdatedEmail;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var newToken = CreateToken(user);

            return Ok(new
            {   
                message = "Profil başarıyla güncellendi.",
                token = newToken
            });
        }

        private bool VerifyPasswordHash(string password, byte[] PasswordHash, byte[] PasswordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(PasswordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(15),  
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }



        private void CreatePassordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; 
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    

    
    private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


        [HttpPost("set-budget")]
        public async Task<IActionResult> SetMonthlyBudget(SetBudgetDto dto)
        {
           var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Kullanıcı kimliği bulunamadı.");
            }
            var userId = int.Parse(userIdString);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Kullanıcı bulunamadı.");
            }
            user.MonthlyBudget = dto.Budget;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Aylık bütçe başarıyla güncellendi.",
                newBudget = user.MonthlyBudget
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var isOldPasswordCorrect = VerifyPasswordHash(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt);

            if (!isOldPasswordCorrect)
            {
                return BadRequest("Mevcut şifreniz hatalı.");
            }

            CreatePassordHash(dto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Şifreniz başarıyla güncellendi." });
        }
    }
}

