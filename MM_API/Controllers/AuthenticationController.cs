using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MM_API.Database.Postgres;
using SharedNetworkFramework.Authentication.RefreshToken;
using SharedNetworkFramework.Authentication.Register;
using SharedNetworkFramework.Authentication.Login;
using SharedNetworkFramework.Authentication.Logout;

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
        //[RequireHttps]
        [HttpPost("register")]
        public async Task<ActionResult<IRegistrationResponse>> RegisterAsync([FromBody] RegistrationPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = new ApplicationUser { UserName = payload.Email };
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
        //[RequireHttps]
        [HttpPost("login")]
        public async Task<ActionResult<ILoginResponse>> LoginAsync([FromBody] LoginPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.LoginAsync(payload);
                if (result is ILoginResponse)
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
        //[RequireHttps]
        [Authorize(Policy = "UserPolicy")]
        [HttpPost("logout")]
        public async Task<ActionResult<ILogoutResponse>> LogoutAsync([FromBody] LogoutPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.LogoutAsync(payload);
                if (result is ILogoutResponse)
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
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        //localhost:5223/authentication/refresh
        // [RequireHttps]
        [Authorize(Policy = "UserPolicy")]
        [HttpPost("refresh")]
        public async Task<ActionResult<IRefreshTokenResponse>> RefreshAsync([FromBody] RefreshTokenPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authenticationService.RefreshTokenAsync(payload);
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
                System.Diagnostics.Debug.WriteLine($"Refresh failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
#region Legacy Code
//namespace MM_API.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class TestAuthenticationController : Controller
//    {
//        private readonly IAuthenticationService _authenticationService;
//        //private readonly IConfiguration _configuration;
//        public TestAuthenticationController(IAuthenticationService authenticationService, IConfiguration configuration)
//        {
//            _authenticationService = authenticationService;
//            // _configuration = configuration;
//        }

//        //localhost:5223/authentication/register
//        [HttpPost("register")]
//        public async Task<ActionResult<IRegistrationResponse>> RegisterAsync([FromBody] RegistrationPayload payload)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            try
//            {
//                var result = await _authenticationService.RegisterAsync(payload);
//                if (result is IRegistrationResponse)
//                {
//                    return Ok(result);
//                }
//                else
//                {
//                    return StatusCode(500, "Unexpected Error Occurred"); //incorrect error code - unsure how to handle 
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//                return StatusCode(500, "Internal Server Error");
//            }
//        }

//        //localhost:5223/authentication/login
//        [HttpPost("login")]
//        public async Task<ActionResult<ILoginResponse>> LoginAsync([FromBody] LoginPayload payload)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            try
//            {
//                var result = await _authenticationService.LoginAsync(payload);
//                if (result is ILoginResponse)
//                {
//                    return Ok(result);
//                }
//                else
//                {
//                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
//                    return StatusCode(500, "Unexpected Error Occurred");
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//                return StatusCode(500, "Internal Server Error");
//            }
//        }

//        //localhost:5223/authentication/logout
//        [HttpPost("logout")]
//        public async Task<ActionResult<ILogoutResponse>> LogoutAsync([FromBody] LogoutPayload payload)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            try
//            {
//                var result = await _authenticationService.LogoutAsync(payload);
//                if (result is ILogoutResponse)
//                {
//                    return Ok(result);
//                }
//                else if (result == null)
//                {
//                    System.Diagnostics.Debug.WriteLine($"result == null");
//                    return StatusCode(500, "result == null");
//                }
//                else
//                {
//                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
//                    return StatusCode(500, "Unexpected Error Occurred");
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//                return StatusCode(500, "Internal Server Error");
//            }
//        }

//        //localhost:5223/authentication/refresh
//        [HttpPost("refresh")]
//        public async Task<ActionResult<IRefreshTokenResponse>> RefreshAsync([FromBody] RefreshTokenPayload payload)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            try
//            {
//                var result = await _authenticationService.RefreshTokenAsync(payload);
//                if (result is IRefreshTokenResponse)
//                {
//                    return Ok(result);
//                }
//                else
//                {
//                    System.Diagnostics.Debug.WriteLine($"Unexpected Error Occurred");
//                    return StatusCode(500, "Unexpected Error Occurred");
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//                return StatusCode(500, "Internal Server Error");
//            }
//        }
//    }
//}
#endregion