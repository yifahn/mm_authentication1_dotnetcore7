using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using SharedNetworkFramework.Authentication.Firebase.Register;
using SharedNetworkFramework.Authentication.Firebase.SignIn;
using SharedNetworkFramework.Authentication.Firebase.RefreshToken;
using SharedNetworkFramework.Authentication.Firebase.SignOut;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        //localhost:5223/authentication/register
        [HttpPost("register")]
        public async Task<ActionResult<IRegistrationResponse>> RegisterAsync([FromBody] RegistrationPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.RegisterAsync(payload);
                if (result is IRegistrationResponse)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, "Unexpected Error Occurred"); //incorrect error code - unsure how to handle 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        //localhost:5223/authentication/login
        [HttpPost("login")]
        public async Task<ActionResult<ISignInResponse>> LoginAsync([FromBody] SignInPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.LoginAsync(payload);
                if (result is ISignInResponse)
                {
                    return Ok(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
                    return StatusCode(500, "Unexpected Error Occurred");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        //localhost:5223/authentication/logout
        [HttpPost("logout")]
        public async Task<ActionResult<ISignOutResponse>> LogoutAsync([FromBody] SignOutPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.LogoutAsync(payload);
                if (result is ISignOutResponse)
                {
                    return Ok(result);
                }
                else if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine($"result == null");
                    return StatusCode(500, "result == null");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
                    return StatusCode(500, "Unexpected Error Occurred");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error"); 
            }
        }

        //localhost:5223/authentication/refresh
        [HttpPost("refresh")]
        public async Task<ActionResult<IRefreshTokenResponse>> RefreshAsync([FromBody] RefreshTokenPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.RefreshIdToken(payload);
                if (result is IRefreshTokenResponse)
                {
                    return Ok(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
                    return StatusCode(500, "Unexpected Error Occurred");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}