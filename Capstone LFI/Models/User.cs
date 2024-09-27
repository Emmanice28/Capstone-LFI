using System.ComponentModel.DataAnnotations;

namespace Capstone_LFI.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Position { get; set; }
        public string Pass { get; set; }
        public string UserLevel { get; set; }
    }
}
