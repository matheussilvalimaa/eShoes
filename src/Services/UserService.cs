using eShoes.Authentication;
using eShoes.Models;
using eShoes.Context;
using Microsoft.EntityFrameworkCore;

namespace eShoes.Services
{
    public class UserService
    {
        private readonly eShoesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(eShoesDbContext context, IHttpContextAccessor httpContextAccessor, JwtService jwtService,ILogger<UserService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        //Registers a new user
        public User Register(User user, string roleName, string password)
        {
            _logger.LogInformation($"Registering new user: {user.Username}");

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            if (_context.Users.Any(u => u.Username == user.Username))
                throw new Exception("Username already exists");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email cannot be empty.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = roleName;

            _context.Users.Add(user);
            _context.SaveChanges();

            _logger.LogInformation($"User {user.Username} registered with ID={user.Id}.");
            return user;
        }

        //Logs in a user by checking if user exists and if password is correct.
        public string Login(string username, string password)
        {
            _logger.LogInformation($"Attempting login for {username}");
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);
            if (user == null)
                throw new Exception("User not found");

            // Verify the hashed password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Incorrect password");

            var roles = new List<string> { user.Role };
            var token = _jwtService.GenerateToken(user.Username, roles);

            _logger.LogInformation($"User '{username}' logged in successfully. Roles: {string.Join(", ", roles)}. Token generated.");
            return token;
        }

        //Delete user method. Only Admin can do this 
        public void DeleteUser(int userId)
        {
            _logger.LogInformation($"Trying to delete user with Id = {userId}");

            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            _context.Users.Remove(user);
            _context.SaveChanges();

            _logger.LogInformation($"User with Id = {userId} deleted with success!");
        }

        //Generates a JWT token for the given username, including role claim if admin.
        public string GenerateJwtToken(string username, IEnumerable<string> roles)
        {
            _logger.LogDebug($"Generating JWT for user {username} with roles: {string.Join(",", roles)}");
            return _jwtService.GenerateToken(username, roles);
        }

        //Get the currently logged user from HttpContext.
        public User GetCurrentUser()
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogDebug("No user logged in at the moment.");
                return null;
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);
            return user;
        }
    }
}
