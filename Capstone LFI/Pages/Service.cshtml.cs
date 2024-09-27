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
    public class ServiceModel : PageModel
    {
        private readonly AppDbContext _context;

        public ServiceModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Service> InventoryList { get; set; }

        [BindProperty]
        public Service NewItem { get; set; }

        [BindProperty]
        public Service UpdateItem { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userLevel = await _context.UserLevel.FirstOrDefaultAsync();

            if (userLevel != null && userLevel.ul == "2")
            {
                string script = @"<script>alert('Not Authorized'); window.location.href = '/Home';</script>";
                return Content(script, "text/html");
            }

            InventoryList = await _context.Service.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            if (action == "Add")
            {
                if (string.IsNullOrEmpty(NewItem.Name) || string.IsNullOrEmpty(NewItem.Category))
                {
                    string script = @"<script>alert('Please make sure all required fields are completed.'); window.location.href = '/Service';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Price <= 0)
                {
                    string script = @"<script>alert('Please enter a valid price.'); window.location.href = '/Service';</script>";
                    return Content(script, "text/html");
                }
                if (NewItem.Description == null)
                {
                    NewItem.Description = " ";
                }

                _context.Service.Add(NewItem);
                await _context.SaveChangesAsync();

                await LogAction("Service", "Add", $"Service: {NewItem.Name}, Category: {NewItem.Category}, Price: {NewItem.Price}", NewItem.Description);

                return RedirectToPage("/Service");
            }
            else if (action == "Update")
            {

                if (UpdateItem.Description == null)
                {
                    UpdateItem.Description = " ";
                }
                var itemToUpdate = await _context.Service.FindAsync(UpdateItem.ID);

                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(UpdateItem.Name) || string.IsNullOrEmpty(UpdateItem.Category))
                {
                    string script = @"<script>alert('Please enter valid values.'); window.location.href = '/Service';</script>";
                    return Content(script, "text/html");
                }

                await LogAction("Service", "Update", $"Category change to: {UpdateItem.Category}, Item name change to: {UpdateItem.Name}, Price change to: {UpdateItem.Price}", $"Description change to: {UpdateItem.Description}");

                itemToUpdate.Category = UpdateItem.Category;
                itemToUpdate.Name = UpdateItem.Name;
                itemToUpdate.Description = UpdateItem.Description;
                itemToUpdate.Price = UpdateItem.Price;

                await _context.SaveChangesAsync();

                return RedirectToPage("/Service");
            }
            else if (action == "Delete" && id.HasValue)
            {
                var itemToDelete = await _context.Service.FindAsync(id);

                if (itemToDelete == null)
                {
                    return NotFound();
                }

                _context.Service.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                await LogAction("Service", "Delete", $"Item: {itemToDelete.Name}, Category: {itemToDelete.Category}, Price: {itemToDelete.Price}", itemToDelete.Description);

                return RedirectToPage("/Service");
            }

            return Page();
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
    }
}
