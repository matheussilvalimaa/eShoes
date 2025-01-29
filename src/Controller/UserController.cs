using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using eShoes.Services;
using eShoes.Models;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        //Request model for user register
        public class UserRegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string CompleteName { get; set; }
        }

        //Request model for user login
        public class UserLoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        //Registers a new user.
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] UserRegisterRequest userRequest)
        {
            try
            {
                _logger.LogInformation($"POST /api/user/register -> Registering user '{userRequest.Username}'");
                
                var user = new User
                {
                    Username = userRequest.Username,
                    Email = userRequest.Email,
                    CompleteName = userRequest.CompleteName
                };

                //Define the default role as User
                var registeredUser = _userService.Register(user, "User", userRequest.Password);
                return Ok(new
                {
                    registeredUser.Id, 
                    registeredUser.Username, 
                    registeredUser.Email, 
                    registeredUser.CompleteName,
                    registeredUser.Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Logs in a user and stores the JWT in a 'JWT' cookie. If user is admin, it includes an admin role claim.
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
                    MaxAge = TimeSpan.FromHours(1),
                    Path = "/"
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

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int userId)
        {
            try
            {
                _logger.LogInformation($"DELETE /api/user/{userId} -> Trying to delete user from database");

                //Avoid the Admin to delete himself
                var currentUser = _userService.GetCurrentUser();
                if (currentUser != null && currentUser.Id == userId)
                    return BadRequest(new { message = "You can't delete yourself!"});
                
                _userService.DeleteUser(userId);
                return Ok(new { message = "User deleted successfully"});
            }
            catch (Exception ex)
            {
                return BadRequest (new { message = ex.Message });
            }
        }


        //Gets the profile of the currently logged-in user.
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            _logger.LogInformation("GET /api/user/profile -> Retrieving current user profile.");
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized("No user logged in or user not found.");

            _logger.LogInformation($"Profile data returned for user '{currentUser.Username}'.");
            return Ok(new
            {
                currentUser.Id,
                currentUser.Username,
                currentUser.Email,
                currentUser.CompleteName,
                currentUser.Role
            });
        }

        //Logs out, removing the 'JWT' cookie.
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("GET /api/user/logout -> Logging out user (deleting 'JWT' cookie).");
            Response.Cookies.Delete("JWT");
            return Ok();
        }
    }
}
