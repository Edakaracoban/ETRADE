using Microsoft.AspNetCore.Identity;

namespace ETRADE.WebUI.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
