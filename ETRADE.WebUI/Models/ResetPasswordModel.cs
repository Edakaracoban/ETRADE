using System.ComponentModel.DataAnnotations;

namespace ETRADE.WebUI.Models
{
    public class ResetPasswordModel
    {
        public string Token { get; set; } // Eşleşmeyi sağlamak için Identity'den gelen token
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
