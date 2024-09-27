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
    public class CustomerModel : PageModel
    {
        private readonly AppDbContext _context;

        public CustomerModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Customer> InventoryList { get; set; }

        [BindProperty]
        public Customer NewItem { get; set; }

        [BindProperty]
        public Customer UpdateItem { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userLevel = await _context.UserLevel.FirstOrDefaultAsync();

            if (userLevel != null && userLevel.ul == "2")
            {
                string script = @"<script>alert('Not Authorized'); window.location.href = '/Home';</script>";
                return Content(script, "text/html");
            }

            InventoryList = await _context.Customer.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            if (action == "Add")
            {
                if (string.IsNullOrEmpty(NewItem.Name) ||
                    string.IsNullOrEmpty(NewItem.Address) ||
                    string.IsNullOrEmpty(NewItem.Contact) ||
                    string.IsNullOrEmpty(NewItem.Attendant) ||
                    string.IsNullOrEmpty(NewItem.ProductService) ||
                    string.IsNullOrEmpty(NewItem.Additional))
                {
                    string script = @"<script>alert('Please make sure all required fields are completed.'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Name.Length < 3)
                {
                    string script = @"<script>alert('Please enter a valid name.'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Address.Length < 10)
                {
                    string script = @"<script>alert('Please enter a valid address.'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Contact.Length != 11 || !NewItem.Contact.StartsWith("09"))
                {
                    string script = @"<script>alert('Please enter a valid contact.'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Attendant.Length < 3)
                {
                    string script = @"<script>alert('Please enter a valid attendant'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.ProductService.Length < 5)
                {
                    string script = @"<script>alert('Please enter a valid productservice'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Additional.Length < 5)
                {
                    string script = @"<script>alert('Please enter a valid additional'); window.location.href = '/Customer';</script>";
                    return Content(script, "text/html");
                }

                NewItem.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                NewItem.Date = DateTime.Now.ToString("MM/dd/yy", CultureInfo.InvariantCulture);

                _context.Customer.Add(NewItem);
                await _context.SaveChangesAsync();

                await LogAction("Customer", "Add", 
                    $"Name: {NewItem.Name}, Address: {NewItem.Address}, Contact: {NewItem.Contact}", 
                    $"Attendant: {NewItem.Attendant}, Products and Services: {NewItem.ProductService}, Additional: {NewItem.Additional}");

                return RedirectToPage("/Customer");
            }
            else if (action == "Update")
            {
                var itemToUpdate = await _context.Customer.FindAsync(UpdateItem.ID);
                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                var changes = new List<string>();
                if (itemToUpdate.Name != UpdateItem.Name) changes.Add($"Name changed to: {UpdateItem.Name}");
                if (itemToUpdate.Address != UpdateItem.Address) changes.Add($"Address changed to: {UpdateItem.Address}");
                if (itemToUpdate.Contact != UpdateItem.Contact) changes.Add($"Contact changed to: {UpdateItem.Contact}");
                if (itemToUpdate.Attendant != UpdateItem.Attendant) changes.Add($"Attendant changed to: {UpdateItem.Attendant}");
                if (itemToUpdate.ProductService != UpdateItem.ProductService) changes.Add($"Product changed to: {UpdateItem.ProductService}");
                if (itemToUpdate.Additional != UpdateItem.Additional) changes.Add($"Additional changed to: {UpdateItem.Additional}");

                itemToUpdate.Name = UpdateItem.Name;
                itemToUpdate.Address = UpdateItem.Address;
                itemToUpdate.Contact = UpdateItem.Contact;
                itemToUpdate.Attendant = UpdateItem.Attendant;
                itemToUpdate.ProductService = UpdateItem.ProductService;
                itemToUpdate.Additional = UpdateItem.Additional;
                itemToUpdate.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                itemToUpdate.Date = DateTime.Now.ToString("MM/dd/yy", CultureInfo.InvariantCulture);

                await _context.SaveChangesAsync();

                await LogAction("Customer", "Update", string.Join(", ", changes), string.Empty);
                return RedirectToPage("/Customer");
            }
            else if (action == "Delete" && id.HasValue)
            {
                var itemToDelete = await _context.Customer.FindAsync(id);
                if (itemToDelete == null)
                {
                    return NotFound();
                }

                _context.Customer.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                await LogAction("Customer", "Delete", 
                    $"Name: {itemToDelete.Name}, Address: {itemToDelete.Address}, Contact: {itemToDelete.Contact}", 
                    $"Attendant: {itemToDelete.Attendant}, Products and Services: {itemToDelete.ProductService}, Additional: {itemToDelete.Additional}");

                return RedirectToPage("/Customer");
            }

            return Page();
        }

        private async Task LogAction(string module, string action, string info, string subInfo)
        {
            var logEntry = new Log
            {
                Module = module,
                Action = action,
                Info = info,
                SubInfo = subInfo,
                Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                Date = DateTime.Now.ToString("MM/dd/yy", CultureInfo.InvariantCulture)
            };

            _context.Log.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
