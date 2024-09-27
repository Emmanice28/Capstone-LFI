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
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerContact { get; set; }
        public string AttendantName { get; set; }
        public string ProductService { get; set; }

        public class PurchaseItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
        }

        public void OnGet(string time, string date, string purchaseItems, string totalQuantity, string totalSession,
            string totalAmount, string paymentMethod, string paymentReference, string change, string customerName,
            string customerAddress, string customerContact, string attendantName)
        {
            ReferenceNumber = GenerateUniqueReferenceNumber();
            Time = time;
            Date = date;
            PurchaseItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PurchaseItem>>(purchaseItems);

            if (totalSession == "0")
            {
                ProductService = string.Join(", ", PurchaseItems.Select(item => $"Name: {item.Name}, Quantity: {item.Quantity}, Price: {item.Price}, Total: {item.Total}"));
            }
            else if (totalQuantity == "0")
            {
                ProductService = string.Join(", ", PurchaseItems.Select(item => $"Name: {item.Name}, Session: {item.Quantity}, Price: {item.Price}, Total: {item.Total}"));
            }
            else
            {
                ProductService = string.Join(", ", PurchaseItems.Select(item => $"Name: {item.Name}, Quantity/Session: {item.Quantity}, Price: {item.Price}, Total: {item.Total}"));
            }
            
            if (totalQuantity == "0")
            {
                TotalQuantity = " ";
            }
            else
            {
                TotalQuantity = string.IsNullOrEmpty(totalQuantity) ? " " : totalQuantity;
            }
            if (totalSession == "0")
            {
                TotalSession = " ";
            }
            else
            {
                TotalSession = string.IsNullOrEmpty(totalSession) ? " " : totalSession;
            }
            TotalAmount = string.IsNullOrEmpty(totalAmount) ? " " : totalAmount;
            PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? " " : paymentMethod;
            PaidRefNo = string.IsNullOrEmpty(paymentReference) ? " " : (paymentReference.Length >= 8 ? "Reference Number: " + paymentReference : "Paid Amount: " + paymentReference);
            Change = string.IsNullOrEmpty(change) ? "None" : new string(change.SkipWhile(c => !char.IsDigit(c)).ToArray());
            CustomerName = string.IsNullOrEmpty(customerName) ? "None" : customerName;
            CustomerAddress = string.IsNullOrEmpty(customerAddress) ? " " : customerAddress;
            CustomerContact = string.IsNullOrEmpty(customerContact) ? " " : customerContact;
            AttendantName = string.IsNullOrEmpty(attendantName) ? " " : attendantName;

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
