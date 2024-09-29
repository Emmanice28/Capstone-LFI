using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Newtonsoft.Json;

namespace Capstone_LFI.Pages
{
    public class POSModel : PageModel
    {
        private readonly AppDbContext _context;

        public POSModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Inventory> InventoryItems { get; set; }
        public List<Service> ServiceItems { get; set; }
        public List<Attendant> AttendantItems { get; set; }
        [BindProperty]
        public Attendant NewItemAttendant { get; set; }

        public async Task OnGetAsync()
        {
            AttendantItems = await _context.Attendant.ToListAsync();
            InventoryItems = await _context.Inventory.ToListAsync();
            ServiceItems = await _context.Service.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "Pay")
            {
                var attendantName = Request.Form["attendantName"].ToString();
                var customerName = Request.Form["customerName"].ToString();
                var customerAddress = Request.Form["customerAddress"].ToString();
                var customerContact = Request.Form["customerContact"].ToString();
                var payment = Request.Form["payment"].ToString();
                var paid = Request.Form["paid"].ToString();
                var discount = Request.Form["discount"].ToString();
                var tableItemsValue = Request.Form["TableItems"].ToString();
                var htableItemsValue = Request.Form["HTableItems"].ToString();

                int totalQuantity = 0;
                int totalSession = 0;
                decimal totalAmount = 0;

                var purchaseDetails = JsonConvert.DeserializeObject<List<PurchaseItem>>(tableItemsValue);
                foreach (var item in purchaseDetails)
                {
                    var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(i => i.Name == item.Name);
                    if (inventoryItem != null)
                    {
                        inventoryItem.Quantity -= item.Quantity;
                        _context.Inventory.Update(inventoryItem);
                        totalQuantity += item.Quantity;
                    }
                    else
                    {
                        totalSession += item.Quantity;
                    }
                    totalAmount += item.Total;
                }

                var hpurchaseDetails = JsonConvert.DeserializeObject<List<HPurchaseItem>>(htableItemsValue);
                foreach (var item in hpurchaseDetails)
                {
                    Console.WriteLine($"Category: {item.HCategory}, Name: {item.HName}, Description: {item.HDescription}, Quantity: {item.HQuantity}");
                }

                await _context.SaveChangesAsync();

                var attendant = await _context.Attendant.FirstOrDefaultAsync(a => a.Name == attendantName);
                if (attendant != null)
                {
                    attendant.Status = "Unavailable";
                    _context.Attendant.Update(attendant);
                    await _context.SaveChangesAsync();
                }

                var referenceNumber = new Random().Next(100000000, 999999999).ToString();
                var currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                var currentDate = DateTime.Now.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);

                decimal exactAmount = totalAmount;

                if (!string.IsNullOrWhiteSpace(discount))
                {
                    discount = discount.Trim();
                    if (discount.EndsWith("%"))
                    {
                        var numericPart = discount.TrimEnd('%');
                        if (decimal.TryParse(numericPart, out decimal discountValue))
                        {
                            exactAmount -= exactAmount * (discountValue / 100);
                        }
                    }
                    else if (decimal.TryParse(discount, out decimal flatDiscount))
                    {
                        exactAmount -= flatDiscount;
                    }
                    else
                    {
                        // Log or handle the invalid discount format
                    }
                }

                var tableItems2 = tableItemsValue.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace("\"", "");

                await LogAction("Point of Sales", "Purchase", $"Info: {tableItems2}, MOP: {payment}, Ref No. or Paid Amt.: {paid}", $"Attendant: {attendantName}, Customer: {customerName}, Address: {customerAddress}, Customer: {customerContact}");
                await CustomerLogAction(customerName, customerAddress, customerContact, attendantName, tableItems2, $"MOP: {payment}, RefNo or PaidAmt: {paid}");

                return RedirectToPage("/POS_Receipt", new
                {
                    referenceNumber,
                    time = currentTime,
                    date = currentDate,
                    purchaseItems = tableItemsValue,
                    totalQuantity,
                    totalSession,
                    totalAmount,
                    paymentMethod = payment,
                    paymentReference = paid,
                    change = (payment == "Cash" ? (decimal.Parse(paid) - exactAmount).ToString("C") : "Not Applicable"),
                    customerName,
                    customerAddress,
                    customerContact,
                    attendantName,
                    discount,
                    exactAmount
                });
            }

            return Page();
        }

        private async Task LogAction(string module, string action, string info, string subInfo)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var currentDate = DateTime.Now.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);

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

        private async Task CustomerLogAction(string name, string address, string contact, string attendant, string productservice, string additional)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var currentDate = DateTime.Now.ToString("MM/dd/yy", CultureInfo.InvariantCulture);

            var logEntry = new Customer
            {
                Name = name,
                Address = address,
                Contact = contact,
                Attendant = attendant,
                ProductService = productservice,
                Additional = additional,
                Time = currentTime,
                Date = currentDate
            };

            _context.Customer.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        private class PurchaseItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
        }
        private class HPurchaseItem
        {
            public string HName { get; set; }
            public int HQuantity { get; set; }
            public string HCategory { get; set; }
            public string HDescription { get; set; }
        }
    }
}
