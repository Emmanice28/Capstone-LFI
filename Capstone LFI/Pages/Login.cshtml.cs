using Microsoft.EntityFrameworkCore;
using Capstone_LFI.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Capstone_LFI.Data;

namespace Capstone_LFI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public LoginModel(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string username, string password)
        {
            if (username == null || password == null)
            {
                return RedirectToPage("/Login");
            }
            else
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                string query = "SELECT UserLevel FROM [User] WHERE Username = @Username AND Pass = @Password";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        connection.Open();
                        var userLevel = command.ExecuteScalar();

                        if (userLevel != null)
                        {
                            string updateQuery = "UPDATE TOP(1) UserLevel SET ul = @UserLevel";

                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@UserLevel", userLevel.ToString());
                                updateCommand.ExecuteNonQuery();
                            }

                            TempData["UserLevel"] = userLevel.ToString();
                            await LogAction("Logged", "In", $"Username: {username}", $"Password: {password}");

                            return RedirectToPage("/Home");
                        }
                        else
                        {
                            return RedirectToPage("/Login");
                        }
                    }
                }
            }
        }

        private async Task LogAction(string module, string action, string info, string subInfo)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            var currentDate = DateTime.Now.ToString("MMMM dd, yyyy");

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
    }
}
