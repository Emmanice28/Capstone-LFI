using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Capstone_LFI.Pages
{
    public class Login_CreateAccountModel : PageModel
    {
        private readonly AppDbContext _context;

        public Login_CreateAccountModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string MiddleName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string AuthUsername { get; set; }

        [BindProperty]
        public string AuthPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string NewPassword1 { get; set; }

        [BindProperty]
        public string Position { get; set; }

        [BindProperty]
        public string UserLevel { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var fullname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(LastName.ToLower())}, {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(FirstName.ToLower())}{(MiddleName == "Middle Name (Optional)" ? "" : $" {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MiddleName.ToLower())}")}";
            var newUsername = GenerateUsername(FirstName.ToLower(), LastName.ToLower());
            var position2 = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Position.ToLower())}";
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Username == newUsername);
            var authorizedUser = await _context.User.FirstOrDefaultAsync(u => u.Username == AuthUsername.ToLower() && u.Pass == AuthPassword);

            if (string.IsNullOrEmpty(FirstName) || FirstName == "First Name" || string.IsNullOrEmpty(LastName) || LastName == "Last Name")
            {
                Message = "First and Last name are required";
            }
            else if (existingUser != null)
            {
                Message = "We already have a record for (" + newUsername + ")";
            }
            else if (string.IsNullOrEmpty(Position) || Position == "Position")
            {
                Message = "Position needed";
            }
            else if (string.IsNullOrEmpty(UserLevel))
            {
                Message = "User level needed";
            }
            else if (authorizedUser == null || string.IsNullOrEmpty(AuthUsername) || string.IsNullOrEmpty(AuthPassword))
            {
                Message = "Invalid authorized name or password";
            }
            else if (authorizedUser.UserLevel != "1")
            {
                Message = "User not authorized";
            }
            else if (string.IsNullOrEmpty(NewPassword) || NewPassword == "New Password" || string.IsNullOrEmpty(NewPassword1) || NewPassword1 == "Confirm Password")
            {
                Message = "New password needed";
            }
            else if (NewPassword != NewPassword1)
            {
                Message = "The new passwords do not match.";
            }
            else
            {
                var newUser = new User
                {
                    Fullname = fullname,
                    Username = newUsername,
                    Position = position2,
                    Pass = NewPassword.ToLower(),
                    UserLevel = UserLevel
                };
                _context.User.Add(newUser);
                await _context.SaveChangesAsync();

                // Add log entry
                await AddLogAsync(newUsername, NewPassword, fullname, position2, UserLevel);

                Message = "User created";
            }
            return Page();
        }

        private string GenerateUsername(string firstName, string lastName)
        {
            var initials = firstName.Length > 0 ? firstName[0] : 'e';
            var prefix = UserLevel == "2" ? 'e' : 'm';
            return $"{prefix}-{initials}{lastName}";
        }

        private async Task AddLogAsync(string username, string password, string fullname, string position, string userLevel)
        {
            var log = new Log
            {
                Module = "Login",
                Action = "Create User",
                Info = $"Username: {username}, Password: {password}",
                SubInfo = $"{fullname}, {position}, {userLevel}",
                Time = DateTime.Now.ToString("HH:mm:ss"), // Military time
                Date = DateTime.Now.ToString("MM/dd/yy")  // Date in MM/DD/YY format
            };

            _context.Log.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
