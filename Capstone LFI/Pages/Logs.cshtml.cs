using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Capstone_LFI.Pages
{
    public class LogsModel : PageModel
    {
        private readonly AppDbContext _context;

        public LogsModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Log> LogList { get; set; }

        [BindProperty]
        public Log NewItem { get; set; }

        [BindProperty]
        public Log UpdateItem { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userLevel = await _context.UserLevel.FirstOrDefaultAsync();

            if (userLevel != null && userLevel.ul == "2")
            {
                string script = @"<script>alert('Not Authorized'); window.location.href = '/Home';</script>";
                return Content(script, "text/html");
            }

            LogList = await _context.Log.ToListAsync();
            return Page();
        }


        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            if (action == "Add")
            {
                _context.Log.Add(NewItem);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Logs");
            }
            else if (action == "Update")
            {
                var itemToUpdate = await _context.Log.FindAsync(UpdateItem.ID);

                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                itemToUpdate.Module = UpdateItem.Module;
                itemToUpdate.Action = UpdateItem.Action;
                itemToUpdate.Info = UpdateItem.Info;
                itemToUpdate.SubInfo = UpdateItem.SubInfo;
                itemToUpdate.Time = UpdateItem.Time;
                itemToUpdate.Date = UpdateItem.Date;

                await _context.SaveChangesAsync();

                return RedirectToPage("/Logs");
            }
            else if (action == "Delete" && id.HasValue)
            {
                var itemToDelete = await _context.Log.FindAsync(id);

                if (itemToDelete == null)
                {
                    return NotFound();
                }

                _context.Log.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Logs");
            }

            return Page();
        }
    }
}

