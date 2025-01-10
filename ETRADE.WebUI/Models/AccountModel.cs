using ETRADE.WebUI.Identity;
using System.ComponentModel.DataAnnotations;

namespace ETRADE.WebUI.Models
{
    public class AccountModel : ApplicationUser
    {
        public string FullName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
