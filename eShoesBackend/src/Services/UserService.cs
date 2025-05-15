using eShoes.Authentication;
using eShoes.Models;
using eShoes.Context;
using Microsoft.EntityFrameworkCore;
using Stripe;

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
        public async Task <User> Register(User user, string password)
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {user.Username} registered with ID={user.Id}.");

            //Create Stripe Customer
            _logger.LogInformation("Creating Stripe Customer for new user.");
            var customerOptions = new CustomerCreateOptions
            {
                Email = user.Email,
                Name = user.Username,
                Metadata = new Dictionary<string, string> { { "user_id", user.Id.ToString() } }
            };
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(customerOptions);

            user.StripeCustomerId = customer.Id;
            await _context.SaveChangesAsync();

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

            var token = _jwtService.GenerateToken(user.Username);

            _logger.LogInformation($"User '{username}' logged in successfully. Token generated.");
            return token;
        }

        public object GetProfile(string username)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);
            
            if (user == null)
            {
                _logger.LogWarning($"User {username} not found.");
                return null;
            }

            return new
            {
                user.Username,
                user.Email
            };
        }

        public async Task<UpdateResult> UpdateProfile(string currentUsername, ProfileUpdateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                //Search the current user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == currentUsername);

                if (user == null)
                {
                    _logger.LogWarning($"User {currentUsername} not found.");
                    return new UpdateResult { Success = false, Error = "User not found." };
                }

                //Validate the new username
                if (!string.IsNullOrWhiteSpace(request.NewUsername) && request.NewUsername != currentUsername)
                {
                    if (await _context.Users.AnyAsync(u => u.Username == request.NewUsername))
                    {
                        return new UpdateResult 
                        { 
                            Success = false, 
                            Error = "Username already taken.",
                            ConflictField = "username"
                        };
                    }
                    user.Username = request.NewUsername;
                }

                //Validate the new email
                if (!string.IsNullOrWhiteSpace(request.NewEmail) && request.NewEmail != user.Email)
                {
                    if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail))
                    {
                        return new UpdateResult 
                        { 
                            Success = false, 
                            Error = "Email already in use.",
                            ConflictField = "email"
                        };
                    }
                    user.Email = request.NewEmail;
                }

                //Updates the password
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UpdateResult 
                {
                    Success = true,
                    UpdatedUsername = user.Username,
                    UpdatedEmail = user.Email
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating profile for {currentUsername}");
                return new UpdateResult { Success = false, Error = "Internal server error." };
            }
        }

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

public class ProfileUpdateRequest
{
    public string NewUsername { get; set; }
    public string NewEmail { get; set; }
    public string NewPassword { get; set; }
}

public class UpdateResult
{
    public bool Success { get; set; }
    public string UpdatedUsername { get; set; }
    public string UpdatedEmail { get; set; }
    public string Error { get; set; }
    public string ConflictField { get; set; }
}
