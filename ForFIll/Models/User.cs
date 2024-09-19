using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ForFIll.Models
{
  
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(8, ErrorMessage = "Name length can't be more than 8.")]

        public string Username { get; set; }
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]

        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string Password2 { get; set; }



    }
}
