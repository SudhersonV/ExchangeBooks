using System.ComponentModel.DataAnnotations;

namespace IdSrv.Host.ViewModels
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string FromEmail { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Minimum 5 chars")]
        public string FromName { get; set; }
        
        [Required]
        [MinLength(10, ErrorMessage = "Minimum 20 chars")]
        public string Body { get; set; }

        public bool IsEmailSent { get; set; }
    }
}