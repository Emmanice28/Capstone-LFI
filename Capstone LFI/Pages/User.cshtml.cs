using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

namespace Capstone_LFI.Pages
{
    public class UserModel : PageModel
    {
        private readonly AppDbContext _context;

        public UserModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<User> UserList { get; set; }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string MiddleName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public User NewUser { get; set; }

        [BindProperty]
        public User UpdateUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userLevel = await _context.UserLevel.FirstOrDefaultAsync();

            if (userLevel != null && userLevel.ul == "2")
            {
                string script = @"<script>alert('Not Authorized'); window.location.href = '/Home';</script>";
                return Content(script, "text/html");
            }

            UserList = await _context.User.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            string validationMessage = ValidateUserInput(action);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                return ShowAlert(validationMessage);
            }

            if (action == "Add")
            {
                var fullname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(LastName.ToLower())}, {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(FirstName.ToLower())}{(string.IsNullOrEmpty(MiddleName) ? "" : $" {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MiddleName.ToLower())}")}";
                NewUser.Fullname = fullname;
                NewUser.Username = GenerateUsername(FirstName.ToLower(), LastName.ToLower(), NewUser.UserLevel);

                _context.User.Add(NewUser);
                await _context.SaveChangesAsync();

                await LogAction("User", "Add", $"Fullname: {NewUser.Fullname}, Username: {NewUser.Username}, Position: {NewUser.Position}, User Level: {NewUser.UserLevel}", NewUser.Pass);

                return RedirectToPage("/User");
            }
            else if (action == "Update")
            {
                var userToUpdate = await _context.User.FindAsync(UpdateUser.ID);

                if (userToUpdate == null)
                {
                    return NotFound();
                }

                userToUpdate.Fullname = UpdateUser.Fullname;
                userToUpdate.Username = UpdateUser.Username.ToLower();
                userToUpdate.Position = UpdateUser.Position;
                userToUpdate.Pass = UpdateUser.Pass.ToLower();
                userToUpdate.UserLevel = UpdateUser.UserLevel;

                await LogAction("User", "Update", $"Username change to: {UpdateUser.Username}, Position change to: {UpdateUser.Position}, User Level change to: {UpdateUser.UserLevel}", $"Password change to: {UpdateUser.Pass}");

                await _context.SaveChangesAsync();
                return RedirectToPage("/User");
            }
            else if (action == "Delete" && id.HasValue)
            {
                var userToDelete = await _context.User.FindAsync(id);

                if (userToDelete == null)
                {
                    return NotFound();
                }

                await LogAction("User", "Delete", $"Fullname: {userToDelete.Fullname}, Username: {userToDelete.Username}, Position: {userToDelete.Position}, User Level: {userToDelete.UserLevel}", userToDelete.Pass);

                _context.User.Remove(userToDelete);
                await _context.SaveChangesAsync();
                return RedirectToPage("/User");
            }

            return Page();
        }

        private string ValidateUserInput(string action)
        {
            if (action == "Add")
            {
                if (string.IsNullOrEmpty(FirstName) ||
                    string.IsNullOrEmpty(LastName) ||
                    string.IsNullOrEmpty(NewUser.Position) ||
                    string.IsNullOrEmpty(NewUser.Pass) ||
                    string.IsNullOrEmpty(NewUser.UserLevel))
                {
                    return "Please make sure all required fields are completed.";
                }
                else if (FirstName.Length < 3)
                {
                    return "Please enter a valid first name";
                }
                else if (LastName.Length < 3)
                {
                    return "Please enter a valid last name.";
                }
                else if (NewUser != null && NewUser.Position.Length < 3)
                {
                    return "Please enter a valid position.";
                }
                else if (NewUser != null && NewUser.Pass.Length < 5)
                {
                    return "Please enter a valid password, must be at least 5 characters.";
                }
                else if (NewUser != null && NewUser.UserLevel != "1" && NewUser.UserLevel != "2")
                {
                    return "Please enter a valid user level, must only be either 1 or 2.";
                }
            }


            return null;
        }

        private IActionResult ShowAlert(string message)
        {
            string script = $"<script>alert('{message}'); window.location.href = '/User';</script>";
            return Content(script, "text/html");
        }

        private async Task LogAction(string module, string action, string info, string subInfo)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var currentDate = DateTime.Now.ToString("MM/dd/yy", CultureInfo.InvariantCulture);

            var logEntry = new Log
            {
                Module = module,
                Action = action,
                Info = info,
                SubInfo = subInfo,
                Time = currentTime,
                Date = currentDate
            };

            _context.Log.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        private string GenerateUsername(string firstName, string lastName, string userLevel)
        {
            var initials = firstName.Length > 0 ? firstName[0] : 'e';
            var prefix = userLevel == "2" ? 'e' : 'm';
            return $"{prefix}-{initials}{lastName}";
        }
    }
}
