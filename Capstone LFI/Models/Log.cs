using System.ComponentModel.DataAnnotations;

namespace Capstone_LFI.Models
{
    public class Log
    {
        public int ID { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string Info { get; set; }
        public string SubInfo { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
}
