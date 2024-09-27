using System.ComponentModel.DataAnnotations;

namespace Capstone_LFI.Models
{
    public class Inventory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } 
        public string QrCodeBase64 { get; set; }
        public string ImageBase64 { get; set; }
    }

}
