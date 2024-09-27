using System.ComponentModel.DataAnnotations;

namespace Capstone_LFI.Models
{
    public class Receipt
    {
        public int ID { get; set; }
        public string ReferenceNumber { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public string ProductService { get; set; }
        public string TotalQuantity { get; set; }
        public string TotalSession { get; set; }
        public string TotalAmount { get; set; }
        public string MOP { get; set; }
        public string PaidRefNo { get; set; }
        public string Change { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerContact { get; set; }
        public string AttendantName { get; set; }
    }
}
