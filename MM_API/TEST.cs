
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace MM_API
{
   /* [ApiController]  
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly string _jwtSecret = "your-secret-key"; // Replace with your actual secret key

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Authenticate user with Firebase or your preferred method
                var user = await AuthenticateUserAsync(loginRequest.Email, loginRequest.Password);

                // Check if authentication is successful
                if (user != null)
                {
                    // Generate JWT token
                    var token = GenerateJwtToken(user.UserId); // You can customize this method

                    // Return the token to the client
                    return Ok(new { Message = "Login successful", AuthToken = token });
                }
                else
                {
                    // Authentication failed
                    return BadRequest(new { Message = "Login failed" });
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        private async Task<User> AuthenticateUserAsync(string email, string password)
        {
            // Implement your user authentication logic here
            // This could involve checking credentials against a database, external service, etc.
            // Return a User object if authentication is successful, or null otherwise

            // Example:
            // var user = await YourAuthenticationService.AuthenticateUserAsync(email, password);
            // return user;
            return null;
        }

        private string GenerateJwtToken(string userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            // Add additional claims as needed
        };

            var token = new JwtSecurityToken(
                issuer: "your-issuer",
                audience: "your-audience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // AuthToken expiration time
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
    }*/

}
