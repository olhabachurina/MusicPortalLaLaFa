using System.ComponentModel.DataAnnotations;

namespace BestMusPortal.Models
{
    public class User
    {
        public int UserId { get; set; }

        
        public string UserName { get; set; }

       
        public string Email { get; set; }

       
        public string Password { get; set; }

        public bool IsActive { get; set; }

        public bool IsApproved { get; set; }

       
        public string Role { get; set; } = "User"; 

        public List<Song> Songs { get; set; } = new List<Song>(); 
    }
}