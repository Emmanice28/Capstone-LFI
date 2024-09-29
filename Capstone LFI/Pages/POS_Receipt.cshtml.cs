using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
using Capstone_LFI.Models;
using System.Linq;

namespace Capstone_LFI.Pages
{
    public class POS_ReceiptModel : PageModel
    {
        private readonly AppDbContext _context;

        public POS_ReceiptModel(AppDbContext context)
        {
            _context = context;
        }

        public string ReferenceNumber { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public List<PurchaseItem> PurchaseItems { get; set; }
        public string TotalQuantity { get; set; }
        public string TotalSession { get; set; }
        public string TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaidRefNo { get; set; }
        public string Change { get; set; }
        public string Discount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerContact { get; set; }
        public string AttendantName { get; set; }
        public string ProductService { get; set; }
        public string ExactAmount { get; set; }

        public class PurchaseItem
        {
            public string Name { get; set; }
            public int? Quantity { get; set; }
            public decimal? Price { get; set; }
            public decimal? Total { get; set; }
        }

        public void OnGet(string time, string date, string purchaseItems, string totalQuantity, string totalSession,
            string totalAmount, string paymentMethod, string paymentReference, string change, string customerName,
            string customerAddress, string customerContact, string attendantName, string discount, string exactAmount)
        {
            ReferenceNumber = GenerateUniqueReferenceNumber();
            Time = time;
            Date = date;

            PurchaseItems = string.IsNullOrEmpty(purchaseItems)
                ? new List<PurchaseItem>()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<PurchaseItem>>(purchaseItems) ?? new List<PurchaseItem>();

            ProductService = string.Join(", ", PurchaseItems.Select(p => p.Name).Where(n => !string.IsNullOrEmpty(n)));

            TotalQuantity = totalQuantity ?? "0";
            TotalSession = totalSession ?? "0";
            TotalAmount = totalAmount ?? "0";
            PaymentMethod = paymentMethod ?? "Not Specified";
            PaidRefNo = string.IsNullOrEmpty(paymentReference)
                ? "Not Provided"
                : (paymentReference.Length >= 8 ? "Reference Number: " + paymentReference : "Paid Amount: " + paymentReference);
            Change = string.IsNullOrEmpty(change) ? "None" : new string(change.SkipWhile(c => !char.IsDigit(c)).ToArray());
            CustomerName = customerName ?? "None";
            CustomerAddress = customerAddress ?? " ";
            CustomerContact = customerContact ?? " ";
            AttendantName = attendantName ?? " ";
            Discount = discount ?? "None";
            ExactAmount = exactAmount ?? "0";

            AddDataToDatabase();
        }

        private string GenerateUniqueReferenceNumber()
        {
            string newReferenceNumber;
            do
            {
                newReferenceNumber = GenerateReferenceNumber();
            } while (_context.Receipt.Any(r => r.ReferenceNumber == newReferenceNumber));

            return newReferenceNumber;
        }

        private string GenerateReferenceNumber()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private void AddDataToDatabase()
        {
            var newReceipt = new Receipt
            {
                ReferenceNumber = ReferenceNumber,
                Time = Time,
                Date = Date,
                ProductService = ProductService,
                TotalQuantity = TotalQuantity,
                TotalSession = TotalSession,
                TotalAmount = TotalAmount,
                MOP = PaymentMethod,
                PaidRefNo = PaidRefNo,
                Change = Change.Replace("?", ""),
                CustomerName = CustomerName,
                CustomerAddress = CustomerAddress,
                CustomerContact = CustomerContact,
                AttendantName = AttendantName
            };

            _context.Receipt.Add(newReceipt);
            _context.SaveChanges();
        }

        public IActionResult OnPost(string action)
        {
            if (action == "Back")
            {
                return RedirectToPage("/POS");
            }
            return Page();
        }
    }
}
