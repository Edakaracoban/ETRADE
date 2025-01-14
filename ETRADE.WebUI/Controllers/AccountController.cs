using ETRADE.Business.Abstract;
using ETRADE.WebUI.EmailService;
using ETRADE.WebUI.Extensions;
using ETRADE.WebUI.Identity;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace ETRADE.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private ICartService _cartService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                //generate email confirmation token //emaile onay maili gönderme
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                string siteUrl = "https://localhost:7076";
                string activeUrl = $"{siteUrl}{callbackUrl}";
                //send email
                string body = $"Hesabınızı onaylayınız. <br> <br> Lütfen email hesabını onaylamak için linke <a href='{activeUrl}'> tıklayınız.</a>"; //tıklayınıza hperlink oluşturur.

                //Email Service
                MailHelper.SendEmail(body, model.Email, "ETRADE Hesap Aktifleştirme Onayı");//mailhelper sınıfı static olduğu için direkt adı ile erişim sağlayabiliriz.
                return RedirectToAction("Login", "Account");
            }
            
            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token) //callback url dönecek.
        {
            if (userId == null || token == null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Geçersiz Token",
                    Message = "Hesap Onay Bilgileri Yanlış",
                    Css = "danger"
                });
                return Redirect("~");//anasayfaya yönlendir.
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token); // email confirmed ı 1 yapar.
                if (result.Succeeded)
                {
                    _cartService.InitialCart(userId);
                    TempData.Put("message", new ResultModel()
                    {
                        Title = "Hesap Onayı",
                        Message = "Hesabınız Onaylandı",
                        Css = "success"
                    });
                    return RedirectToAction("Login", "Account");//login sayfasına yönlendir.
                }
            }
            TempData.Put("message", new ResultModel()
            {
                Title = "Hesap Onayı",
                Message = "Hesabınız Onaylanmamıştır",
                Css = "danger"
            });
            return Redirect("~");//anasayfaya yönlendir.
        }
    }
}
