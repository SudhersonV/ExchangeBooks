using System;
using System.ComponentModel.DataAnnotations;

namespace IdSrv.Host.Models
{
    public class RegisterUserModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required]        
        [MinLength(8, ErrorMessage = "Password should be minimum 8 chars")]
        public string Password1 { get; set; }
        [Required]
        [Compare(nameof(Password1), ErrorMessage = "Passwords do not match")]
        public string Password2 { get; set; }
        public string ReturnUrl { get; set; }
    }
}
