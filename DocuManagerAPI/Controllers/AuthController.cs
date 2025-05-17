using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocuManagerAPI.Data;
using DocuManagerAPI.Models;

namespace DocuManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public AuthController(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserDtocs user)
    {
        var userDb = FakeUserRepository.GetUserByEmail(user.Email);
        if (userDb == null)
            return Unauthorized(new { mensagem = "Usuário não encontrado" });

        var hasher = new PasswordHasher<object>();
        var result = hasher.VerifyHashedPassword(null, userDb.SenhaHash, user.Senha);

        if (result != PasswordVerificationResult.Success)
            return Unauthorized(new { mensagem = "Senha incorreta" });

        // Gerar token JWT
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Adiciona o JWT como cookie
        Response.Cookies.Append("DocuToken", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // use HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        });

        return Ok(new { mensagem = "Login bem-sucedido, cookie emitido." });
    }
}
