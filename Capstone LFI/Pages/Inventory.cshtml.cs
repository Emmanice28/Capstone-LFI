using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone_LFI.Data;
using Capstone_LFI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Capstone_LFI.Pages
{
    public class InventoryModel : PageModel
    {
        private readonly AppDbContext _context;

        public InventoryModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Inventory> InventoryList { get; set; }

        [BindProperty]
        public Inventory NewItem { get; set; }

        [BindProperty]
        public Inventory UpdateItem { get; set; }

        [BindProperty]
        public IFormFile NewItemImage { get; set; }

        [BindProperty]
        public IFormFile UpdateItemImage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userLevel = await _context.UserLevel.FirstOrDefaultAsync();

            if (userLevel != null && userLevel.ul == "2")
            {
                string script = @"<script>alert('Not Authorized'); window.location.href = '/Home';</script>";
                return Content(script, "text/html");
            }

            InventoryList = await _context.Inventory.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? id)
        {
            if (action == "Add")
            {
                if (string.IsNullOrEmpty(NewItem.Category) || string.IsNullOrEmpty(NewItem.Name) || string.IsNullOrEmpty(NewItem.Code))
                {
                    string script = @"<script>alert('Please make sure all required fields are completed.'); window.location.href = '/Inventory';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Category.Length < 3)
                {
                    string script = @"<script>alert('Please enter a valid category.'); window.location.href = '/Inventory';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Name.Length < 3)
                {
                    string script = @"<script>alert('Please enter a valid name.'); window.location.href = '/Inventory';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Code.Length < 10)
                {
                    string script = @"<script>alert('Please enter a valid code.'); window.location.href = '/Inventory';</script>";
                    return Content(script, "text/html");
                }
                else if (NewItem.Price <= 0 && NewItem.Category != "Others")
                {
                    string script = @"<script>alert('Please enter a valid price.'); window.location.href = '/Inventory';</script>";
                    return Content(script, "text/html");
                }

                if (NewItem.Description == null)
                {
                    NewItem.Description = " ";
                }
                if (NewItem.Price == null)
                {
                    NewItem.Price = 0;
                }
                if (NewItem.Quantity == null)
                {
                    NewItem.Quantity = 0;
                }

                string qrCodeText = $"{NewItem.Category} || {NewItem.Name} || {NewItem.Description}";
                NewItem.QrCodeBase64 = GenerateQrCodeBase64(qrCodeText);

                if (NewItemImage != null && NewItemImage.Length > 0)
                {
                    NewItem.ImageBase64 = ConvertImageToBase64(NewItemImage);
                }
                else
                {
                    NewItem.ImageBase64 = GenerateTransparentImageBase64();
                }

                _context.Inventory.Add(NewItem);
                await _context.SaveChangesAsync();

                await LogActionAsync("Inventory", "Add", $"Item: {NewItem.Name}, Category: {NewItem.Category}, Code: {NewItem.Code}, Quantity: {NewItem.Quantity}, Price: {NewItem.Price}", NewItem.Description);

                return RedirectToPage("/Inventory");
            }
            else if (action == "Update")
            {
                if (UpdateItem.Description == null)
                {
                    UpdateItem.Description = " ";
                }
                if (UpdateItem.Price == null)
                {
                    UpdateItem.Price = 0;
                }
                if (UpdateItem.Quantity == null)
                {
                    UpdateItem.Quantity = 0;
                }

                var itemToUpdate = await _context.Inventory.FindAsync(UpdateItem.ID);

                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                var changes = new List<string>();
                if (itemToUpdate.Category != UpdateItem.Category) changes.Add($"Category changed to: {UpdateItem.Category}");
                if (itemToUpdate.Name != UpdateItem.Name) changes.Add($"Name changed to: {UpdateItem.Name}");
                if (itemToUpdate.Description != UpdateItem.Description) changes.Add($"Description changed to: {UpdateItem.Description}");
                if (itemToUpdate.Code != UpdateItem.Code) changes.Add($"Code changed to: {UpdateItem.Code}");
                if (itemToUpdate.Quantity != UpdateItem.Quantity) changes.Add($"Quantity changed to: {UpdateItem.Quantity}");
                if (itemToUpdate.Price != UpdateItem.Price) changes.Add($"Price changed to: {UpdateItem.Price}");

                itemToUpdate.Category = UpdateItem.Category;
                itemToUpdate.Name = UpdateItem.Name;
                itemToUpdate.Description = UpdateItem.Description;
                itemToUpdate.Code = UpdateItem.Code;
                itemToUpdate.Quantity = UpdateItem.Quantity;
                itemToUpdate.Price = UpdateItem.Price;

                string updatedQrCodeText = $"{itemToUpdate.Category} || {itemToUpdate.Name} || {itemToUpdate.Code}";
                itemToUpdate.QrCodeBase64 = GenerateQrCodeBase64(updatedQrCodeText);

                if (UpdateItemImage != null && UpdateItemImage.Length > 0)
                {
                    itemToUpdate.ImageBase64 = ConvertImageToBase64(UpdateItemImage);
                }

                await _context.SaveChangesAsync();
                await LogActionAsync("Inventory", "Update", string.Join(", ", changes), UpdateItem.Description);

                return RedirectToPage("/Inventory");
            }

            else if (action == "Delete" && id.HasValue)
            {
                var itemToDelete = await _context.Inventory.FindAsync(id);
                if (itemToDelete == null)
                {
                    return NotFound();
                }

                _context.Inventory.Remove(itemToDelete);
                await _context.SaveChangesAsync();
                await LogActionAsync("Inventory", "Delete", $"Item: {itemToDelete.Name}, Category: {itemToDelete.Category}, Code: {itemToDelete.Code}, Quantity: {itemToDelete.Quantity}, Price: {itemToDelete.Price}", itemToDelete.Description);

                return RedirectToPage("/Inventory");
            }

            return Page();
        }

        private string GenerateQrCodeBase64(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            using (var bitmap = qrCode.GetGraphic(20, Color.Black, Color.White, true))
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    byte[] byteArray = stream.ToArray();
                    return Convert.ToBase64String(byteArray);
                }
            }
        }

        private string ConvertImageToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        private string GenerateTransparentImageBase64()
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                bitmap.SetPixel(0, 0, Color.Transparent);
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    byte[] byteArray = stream.ToArray();
                    return Convert.ToBase64String(byteArray);
                }
            }
        }

        private async Task LogActionAsync(string module, string action, string info, string subInfo)
        {
            var log = new Log
            {
                Module = module,
                Action = action,
                Info = info.Length > 260 ? info.Substring(0, 260) : info,
                SubInfo = subInfo.Length > 260 ? subInfo.Substring(0, 260) : subInfo,
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Date = DateTime.Now.ToString("mm/dd/yy")
            };

            _context.Log.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
