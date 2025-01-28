using ETRADE.Business.Abstract;
using ETRADE.WebUI.EmailService;
using ETRADE.WebUI.Extensions;
using ETRADE.WebUI.Identity;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
            var result = await _userManager.CreateAsync(user, model.Password);//password hashlenmiş olarak tutulur.

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

        public IActionResult Login(string returnUrl = null) //default değeri null
        {
            return View(
                new LoginModel()
                {
                    ReturnUrl = returnUrl
                }
             );
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            ModelState.Remove("ReturnUrl");
            if (!ModelState.IsValid)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Giriş Bilgileri",
                    Message = "Bilgileriniz Hatalıdır",
                    Css = "danger"
                });
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                ModelState.AddModelError("", "Bu email adresi ile kayıtlı kullanıcı bulunamadı");
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "~/");
            }
            else if (result.IsLockedOut)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Hesap Kilitlendi",
                    Message = "Hesabınız geçici olarak kilitlenmiştir. Lütfen 5 dk sonra tekrar deneyin.",
                    Css = "danger"
                });
                return View(model);
            }

            ModelState.AddModelError("", "Email veya şifre hatalı");

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new ResultModel()
            {
                Title = "Oturum Hesabı Kapatıldı",
                Message = "Hesabınız güvenli bir şekilde sonlandırıldı",
                Css = "success"
            });
            return Redirect("~/");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Şifremi Unuttum",
                    Message = "Lütfen Email adresini boş bırakmayınız",
                    Css = "danger"
                });

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                string siteUrl = "https://localhost:7076";
                string resetUrl = $"{siteUrl}{callbackUrl}";
                //send email
                string body = $"Şifrenizi yenilemek için linke <a href='{resetUrl}'> tıklayınız.</a>"; //tıklayınıza hperlink oluşturur.
                //Email Service
                MailHelper.SendEmail(body, email, "ETRADE Şifre Sıfırlama");//mailhelper sınıfı static olduğu için direkt adı ile erişim sağlayabiliriz.
                TempData.Put("message", new ResultModel()
                {
                    Title = "Şifre Sıfırlama",
                    Message = "Şifre sıfırlama linki email adresinize gönderilmiştir.",
                    Css = "success"
                });
                return RedirectToAction("Login");
            }
            TempData.Put("message", new ResultModel()
            {
                Title = "Şifremi Unuttum",
                Message = "Bu email adresi ile kayıtlı kullanıcı bulunamadı",
                Css = "danger"
            });
            return View();
        }
        public IActionResult ResetPassword(string token)
        {
            if (token == null)
            {
                return RedirectToAction("Home", "Index");
            }

            var model = new ResetPasswordModel { Token = token };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Şifremi Unuttum",
                    Message = "Bu Email adresi ile kullanıcı bulunamadı.",
                    Css = "danger"
                });
                return RedirectToAction("Home", "Index");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            else
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Şifremi Unuttum",
                    Message = "Şifreniz uygun değildir.",
                    Css = "danger"
                });

                return View(model);
            }
        }
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Bağlantı Hatası",
                    Message = "Kullanıcı bilgileri bulunamadı tekrar deneyin.",
                    Css = "danger"
                });
                return View();
            }

            var model = new AccountModel
            {
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(AccountModel model)
        {

            if (!ModelState.IsValid)
            {

                TempData.Put("message", new ResultModel()
                {
                    Title = "Giriş Bilgileri",
                    Message = "Bilgileriniz Hatalıdır",
                    Css = "danger"
                });

                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["message"] = new ResultModel()
                {
                    Title = "Bağlantı Hatası",
                    Message = "Kullanıcı bilgileri bulunamadı, lütfen tekrar deneyin.",
                    Css = "danger"
                };
                return RedirectToAction("Login", "Account");
            }




            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = model.Email;

            if (model.Email != user.Email)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                string siteUrl = "https://localhost:7076";
                string resetUrl = $"{siteUrl}{callbackUrl}";
                //send email
                string body = $"Şifrenizi yenilemek için linke <a href='{resetUrl}'> tıklayınız.</a>";

                MailHelper.SendEmail(body, model.Email, "ETRADE Şifre Sıfırlama");
                TempData.Put("message", new ResultModel()
                {
                    Title = "Şifre Sıfırlama",
                    Message = "Şifre sıfırlama linki email adresinize gönderilmiştir.",
                    Css = "success"
                });
                return RedirectToAction("Login");
            }

            var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);//kullanıcı bilgilerini güncelledikten sonra oturumu günceller.
                TempData.Put("message", new ResultModel()
                {
                    Title = "Hesap Bilgileri Güncellendi",
                    Message = "Bilgileriniz başarıyla güncellenmiştir.",
                    Css = "success"
                });
                return RedirectToAction("Index", "Home");
            }

            TempData.Put("message", new ResultModel()
            {
                Title = "Hata",
                Message = "Bilgileriniz güncellenemedi. Lütfen tekrar deneyin.",
                Css = "danger"
            });
            return View(model);
        }

    }


}

