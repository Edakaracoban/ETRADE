using Microsoft.AspNetCore.Identity;

namespace ETRADE.WebUI.Identity
{
    public static class SeedIdentity //Static sınıfların instance'ı oluşturulamaz.Static sınıflarda static metotlar kullanılabilir.
    {
        //webui çalıştırıldığında kullanıcı Yoksa otomatik olarak bu bilgilerle kullanıcı oluşturulacak.
        public static async Task Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            var username=configuration["Data:AdminUser:username"];
            var email = configuration["Data:AdminUser:email"];
            var password = configuration["Data:AdminUser:password"];
            var role = configuration["Data:AdminUser:role"];

            if (await userManager.FindByEmailAsync(email) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                var user = new ApplicationUser()
                {
                    UserName = username,
                    Email = email,
                    FullName="Eda Karaçoban",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
