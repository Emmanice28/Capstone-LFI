using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System.Threading.Tasks;

namespace Capstone_LFI.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly AppDbContext _context;

        public ForgotPasswordModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string AuthUsername { get; set; }

        [BindProperty]
        public string AuthPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string NewPassword1 { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == Username);
            if (Username == "Username")
            {
                Message = "Username needed";
            }
            else if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                Message = "No user found with username (" + Username + ")";
            }
            else
            {
                var authorizedUser = await _context.User.FirstOrDefaultAsync(u => u.Username == AuthUsername && u.Pass == AuthPassword);
                if (authorizedUser == null || string.IsNullOrEmpty(AuthUsername) || string.IsNullOrEmpty(AuthPassword))
                {
                    Message = "Invalid username or password";
                }
                else if (authorizedUser.UserLevel != "1")
                {
                    Message = "User (" + AuthUsername + ") is not authorized";
                }
                else if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword == "New Password")
                {
                    Message = "New password needed";
                }
                else if (NewPassword != NewPassword1)
                {
                    Message = "The new passwords do not match.";
                }
                else
                {
                    user.Pass = NewPassword;
                    await _context.SaveChangesAsync();


                    await AddLogAsync(Username, NewPassword);

                    Message = "Password updated";
                }
            }
            return Page();
        }

        private async Task AddLogAsync(string username, string newPassword)
        {
            var log = new Log
            {
                Module = "Login",
                Action = "Password Reset",
                Info = $"Username: {username}",
                SubInfo = $"New Password: {newPassword}",
                Time = DateTime.Now.ToString("HH:mm:ss"), 
                Date = DateTime.Now.ToString("MM/dd/yy") 
            };

            _context.Log.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
