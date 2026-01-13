using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Migrations;
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using Bixd.Models.DTO.AuthDataDto;
using EmployeeService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

public class HomeController( ApplicationDbContext _context, ILogger<HomeController> _logger,
    IWebHostEnvironment _environment,IConfiguration _configuration) : Controller
{
    
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Index(string returnUrl = "/")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return Redirect("/");
        }
        var filePath = Path.Combine(_environment.WebRootPath, "index.html");
        return PhysicalFile(filePath, "text/html");
    }
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] AuthDataDto data)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var authData = await _context.AuthData
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Login == data.Login);
            
        if (authData == null)
        {
            _logger.LogWarning($"Failed login attempt for user: {data.Login}");
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        
        if (data.Password != authData.Password)
        {
            _logger.LogWarning($"Invalid password for user: {data.Login}");
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        
        var employee = await _context.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Id == authData.Id);
            
        if (employee?.Role == null)
            return Unauthorized(new { Message = "User data incomplete" });
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Name, data.Login),
            new Claim(ClaimTypes.Role, employee.Role.Name),
            new Claim("EmployeeId", employee.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);
            
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        Response.Cookies.Append("access_token", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(), 
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(2),
            Path = "/"
        });

        _logger.LogInformation($"User {data.Login} logged in successfully");
        
        return Ok(new 
        {
            Username = data.Login,
            Role = employee.Role.Name,
            EmployeeId = employee.Id,
            Token = tokenString // Для отладки
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during login");
        return StatusCode(500, new { Message = "Internal server error" });
    }
}

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(new { Message = "Logged out successfully" });
    }
    
    [Authorize(Roles = "Сотрудник кадровой службы")]
    [HttpGet("/au")]
    public IActionResult Au()
    {
        // Получаем информацию о текущем пользователе
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
    
        return Ok(new 
        {
            Message = "Au",
            UserId = userId,
            Username = username,
            Role = role
        });
    }


}