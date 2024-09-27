using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Models;
using Capstone_LFI.Data;

namespace Capstone_LFI.Pages
{
    public class ReceiptModel : PageModel
    {
        private readonly AppDbContext _context;

        public ReceiptModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Receipt> Receipts { get; set; }

        public void OnGet()
        {
            Receipts = _context.Receipt.ToList();
        }
    }
}
