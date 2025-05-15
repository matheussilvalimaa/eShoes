using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using eShoes.Services;
using eShoes.Models;
using eShoes.DTO;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        //Registers a new user.
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRequest)
        {
            try
            {
                _logger.LogInformation($"POST /api/user/register -> Registering user '{userRequest.Username}'");
                
                var user = new User
                {
                    Username = userRequest.Username,
                    Email = userRequest.Email,
                };

                var registeredUser = await _userService.Register(user, userRequest.Password);
                return Ok(new
                {
                    registeredUser.Id, 
                    registeredUser.Username, 
                    registeredUser.Email, 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Logs in a user and stores the JWT in a 'JWT' cookie. 
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            try
            {
                _logger.LogInformation($"POST /api/user/login -> Attempt login for '{request.Username}'");
                var token = _userService.Login(request.Username, request.Password);
                Response.Cookies.Append("JWT", token, new CookieOptions
                {
                    HttpOnly = true,
                    MaxAge = TimeSpan.FromHours(12),
                    Path = "/",
                    SameSite = SameSiteMode.None,
                    Secure = true,
                });

                _logger.LogInformation($"User '{request.Username}' logged in. Token stored in cookie 'JWT'.");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Login failed.");
                return Unauthorized(new { ex.Message });
            }
        }

        //Logs out, removing the 'JWT' cookie.
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            _logger.LogInformation("GET /api/user/logout -> Logging out user (deleting 'JWT' cookie).");
            Response.Cookies.Delete("JWT");
            return Ok();
        }

        //Get the current user logged
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity.Name;
            return Ok(new { username });
        }

        //Get the current user's profile
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            try
            {
                var username = User.Identity.Name;
                var profile = _userService.GetProfile(username);

                if (profile == null)
                    return NotFound("User not found");
                
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in profile controller");
                return StatusCode(500, "Internal server error.");
            }
        }

        //Update the current user's profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            try
            {
                var currentUsername = User.Identity.Name;
                var result = await _userService.UpdateProfile(currentUsername, request);

                if (!result.Success)
                {
                    return result.ConflictField switch
                    {
                        "username" => Conflict(new { message = result.Error }),
                        "email" => Conflict(new { message = result.Error }),
                        _ => BadRequest(new { message = result.Error })
                    };
                }

                return Ok(new 
                {
                    message = "Profile updated successfully!",
                    username = result.UpdatedUsername,
                    email = result.UpdatedEmail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in profile update controller");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}
