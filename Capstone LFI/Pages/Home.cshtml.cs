using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Capstone_LFI.Pages
{
    public class HomeModel : PageModel
    {
        private readonly AppDbContext _context;

        public HomeModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Inventory> LowStockItems { get; set; }
        public IList<Inventory> InventoryList { get; set; }
        public IList<Service> ServiceList { get; set; }
        public IList<Customer> CustomerList { get; set; }
        public IList<AttendantLogs> AttendantLogs { get; set; }
        public List<Attendant> AttendantItems { get; set; }

        [BindProperty]
        public Attendant NewItem { get; set; }

        [BindProperty]
        public Attendant UpdateItem { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            LowStockItems = _context.Inventory.Where(i => i.Quantity <= 10).ToList();
            AttendantItems = await _context.Attendant.ToListAsync();
            InventoryList = await _context.Inventory.ToListAsync();
            AttendantLogs = await _context.AttendantLogs.ToListAsync();
            ServiceList = await _context.Service.ToListAsync();
            CustomerList = await _context.Customer.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            if (action == "Add")
            {
                if (string.IsNullOrEmpty(NewItem.Name) || string.IsNullOrEmpty(NewItem.Status))
                {
                    string script = @"<script>alert('Please make sure all required fields are completed.'); window.location.href = '/Home';</script>";
                    return Content(script, "text/html");
                }

                var existingLogItem = await _context.AttendantLogs
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == NewItem.Name.ToLower());

                if (existingLogItem != null)
                {
                    
                }
                else
                {
                    _context.AttendantLogs.Add(new AttendantLogs { Name = NewItem.Name });
                    await _context.SaveChangesAsync();
                }

                var existingItem = await _context.Attendant
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == NewItem.Name.ToLower());

                if (existingItem != null)
                {
                    string script = @"<script>alert('Duplicate name in attendants.'); window.location.href = '/Home';</script>";
                    return Content(script, "text/html");
                }

                _context.Attendant.Add(NewItem);
                await _context.SaveChangesAsync();

                await LogAction("Home", "Add", $"Attendant: {NewItem.Name}", "");

                return RedirectToPage("/Home");
            }


            else if (action == "Update")
            {
                var itemToUpdate = await _context.Attendant.FindAsync(UpdateItem.ID);

                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                await LogAction("Home", "Update", $"Attendant change to: {UpdateItem.Name}", "");

                itemToUpdate.Name = UpdateItem.Name;
                itemToUpdate.Status = UpdateItem.Status;

                await _context.SaveChangesAsync();

                return RedirectToPage("/Home");
            }
            else if (action == "Delete" && id.HasValue)
            {
                var itemToDelete = await _context.Attendant.FindAsync(id);

                if (itemToDelete == null)
                {
                    return NotFound();
                }

                _context.Attendant.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                await LogAction("Home", "Delete", $"Attendant: {itemToDelete.Name}", "");

                return RedirectToPage("/Home");
            }

            return Page();
        }

        private async Task LogAction(string module, string action, string info, string subInfo)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var currentDate = DateTime.Now.ToString("mm/dd/yy", CultureInfo.InvariantCulture);

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
