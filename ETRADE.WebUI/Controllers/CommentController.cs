using ETRADE.Business.Abstract;
using ETRADE.Entities;
using ETRADE.WebUI.Identity;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ETRADE.WebUI.Controllers
{
    public class CommentController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ICommentService _commentService;
        private IProductService _productService;

        public CommentController(IProductService productService, UserManager<ApplicationUser> userManager, ICommentService commentService)
        {
            _productService = productService;
            _userManager = userManager;
            _commentService = commentService;
        }
        public IActionResult ShowProductComments(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Product product = _productService.GetProductDetail(id.Value);
            if (product == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var users = new Dictionary<string, string>();
            var validComments = new List<Comment>();
            foreach (var comment in product.Comments)
            {
                // Kullanıcı var mı kontrol et
                var user = _userManager.FindByIdAsync(comment.UserId).Result;
                if (user != null) 
                {
                    
                    if (!users.ContainsKey(comment.UserId))
                    {
                        users[comment.UserId] = user.UserName;
                    }

                    
                    validComments.Add(comment);
                }
            }

          
            ViewBag.Usernames = users;

           
            return PartialView("_PartialComments", validComments);
        }
        public IActionResult Create(CommentModel model, int? productId)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {


                if (productId == null)
                {
                    return BadRequest();
                }

                Product product = _productService.GetById(productId.Value);
                if (product == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                Comment comment = new Comment()
                {
                    ProductId = productId.Value,
                    CreateOn = DateTime.Now,
                    UserId = _userManager.GetUserId(User) ?? "0",
                    Text = model.Text.Trim('\n').Trim(' '),
                };
                _commentService.Create(comment);

                return Json(new { result = true });
            }
            return View(model);
        }
        public IActionResult Edit(int? id, string text)
        {
            if (id is null)
            {
                return BadRequest();
            }

            Comment comment = _commentService.GeyById(id.Value);

            if (comment is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            comment.Text = text;
            comment.CreateOn = DateTime.Now;

            _commentService.Update(comment);

            return Json(new { result = true });
        }
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Comment comment = _commentService.GeyById(id.Value);

            if (comment is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            _commentService.Delete(comment);
            return Json(new { result = true });

        }

    }
}
