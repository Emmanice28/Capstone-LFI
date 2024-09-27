using System.ComponentModel.DataAnnotations;

namespace Capstone_LFI.Models
{
    public class Customer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Attendant { get; set; }
        public string ProductService { get; set; }
        public string Additional { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
}
