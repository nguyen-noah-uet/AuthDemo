using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthDemo.Controllers;
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly IConfiguration _configuration;

	public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
	{
		_userManager = userManager;
		_configuration = configuration;
	}
	// authenticate, register
	[HttpPost]
	public async Task<IActionResult> Authenticate(LoginModel model)
	{
		var user = await _userManager.FindByNameAsync(model.Username);
		if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
		{
			// Đúng username và password
			// Trả về JWT token
			return Ok(GenerateJwtToken(user));
		}

		return Unauthorized();
	}

	private string GenerateJwtToken(IdentityUser user)
	{
		var claims = new Claim[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
			claims: claims,
			notBefore: DateTime.Now,
			expires: DateTime.Now.AddHours(1),
			signingCredentials: new SigningCredentials(
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
				SecurityAlgorithms.HmacSha256)));
		return token;
	}

	[HttpPost]
	[Route("register")] // api/auth/register {"Username": "admin", "Password":"test"}
	public async Task<IActionResult> Register(RegisterModel model)
	{
		IdentityUser user = new IdentityUser() { UserName = model.Username };
		var result = await _userManager.CreateAsync(user, model.Password);
		if (result.Succeeded)
		{
			return Ok("Registered");
		}
		return BadRequest(result.Errors);
	}
}

public record RegisterModel(string Username, string Password);
public record LoginModel(string Username, string Password);