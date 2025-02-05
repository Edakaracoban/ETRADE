using ETRADE.Business.Abstract;
using ETRADE.DataAccess.Concrete.EfCore;
using ETRADE.Entities;
using ETRADE.WebUI.Identity;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System;

namespace ETRADE.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private IProductService _productService;
        private ICategoryService _categoryService;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private IOrderService _orderService;

        public AdminController(IProductService productService, ICategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOrderService orderService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
            _orderService = orderService;
        }


        [Route("admin/products")]
        public IActionResult ProductList()
        {
            return View(
                new ProductListModel()
                {
                    Products = _productService.GetAll()
                }
             );
        }

        public IActionResult CreateProduct()
        {
            var category = _categoryService.GetAll();
            ViewBag.Category = category.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

            return View(new ProductModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductModel model, List<IFormFile> files)
        {
            ModelState.Remove("SelectedCategories");

            if (ModelState.IsValid)
            {
                if (int.Parse(model.CategoryId) == -1)
                {
                    ModelState.AddModelError("", "Lütfen bir kategori seçiniz.");

                    ViewBag.Category = _categoryService.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

                    return View(model);
                }

                var entity = new Product()
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price
                };

                if (files.Count < 4 || files == null)
                {
                    ModelState.AddModelError("", "Lütfen en az 4 resim yükleyin.");
                    ViewBag.Category = _categoryService.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                    return View(model);
                }
                foreach (var item in files)
                {
                    Image image = new Image();
                    image.ImageUrl = item.FileName;

                    entity.Images.Add(image);

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", item.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await item.CopyToAsync(stream);
                    }
                }


                entity.ProductCategories = new List<ProductCategory> { new ProductCategory { CategoryId = int.Parse(model.CategoryId), ProductId = entity.Id } };

                _productService.Create(entity);

                return RedirectToAction("ProductList");
            }

            ViewBag.Category = _categoryService.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });


            return View(model);

        }


        public IActionResult EditProduct(int id)

        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = _productService.GetProductDetail(id);

            if (entity == null)
            {
                return NotFound();
            }

            var model = new ProductModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Images = entity.Images,
                SelectedCategories = entity.ProductCategories.Select(i => i.Category).ToList()
            };

            ViewBag.Categories = _categoryService.GetAll();

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductModel model, List<IFormFile> files, int[] categoryIds)
        {
            if (categoryIds == null || categoryIds.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen en az bir kategori seçiniz.");
                ViewBag.Categories = _categoryService.GetAll();
                return View(model); 
            }

            var entity = _productService.GetById(model.Id);
          

            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.Images = model.Images;

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Image image = new Image();
                    image.ImageUrl = file.FileName;

                    entity.Images.Add(image);

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", file.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            _productService.Update(entity, categoryIds);

            return RedirectToAction("ProductList");
        }

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _productService.GetById(productId);

            if (product != null)
            {
                _productService.Delete(product);
            }

            return RedirectToAction("ProductList");
        }

        public IActionResult CategoryList()
        {
            return View(new CategoryListModel() { Categories = _categoryService.GetAll() });
        }

        public IActionResult EditCategory(int? id)
        {
            var entity = _categoryService.GetByWithProducts(id.Value);

            return View(
                    new CategoryModel()
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Products = entity.ProductCategories.Select(i => i.Product).ToList()
                    }
                );
        }


        [HttpPost]
        public IActionResult EditCategory(CategoryModel model)
        {
            var entity = _categoryService.GetById(model.Id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            _categoryService.Update(entity);

            return RedirectToAction("CategoryList");
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            var entity = _categoryService.GetById(categoryId);
            _categoryService.Delete(entity);

            return RedirectToAction("CategoryList");
        }

        public IActionResult CreateCategory()
        {
            return View(new CategoryModel());
        }


        [HttpPost]
        public IActionResult CreateCategory(CategoryModel model)
        {
            var entity = new Category()
            {
                Name = model.Name
            };

            _categoryService.Create(entity);

            return RedirectToAction("CategoryList");
        }

        public IActionResult GetOrder()
        {
            var userId = _userManager.GetUserId(User); 
            var UserName = _userManager.GetUserName(User);

            var orders = _orderService.GetOrders(userId,UserName);

            var orderListModel = new List<OrderListModel>();

            foreach (var order in orders)
            {
                var orderModel = new OrderListModel()
                {
                    OrderId = order.Id,
                    Address = order.Address,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    OrderState = order.OrderState,
                    PaymentTypes = order.PaymentTypes,
                    OrderNote = order.OrderNote,
                    City = order.City,
                    Email = order.Email,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    Phone = order.Phone,
                    OrderItems = order.OrderItems.Select(x => new OrderItemModel()
                    {
                        OrderItemId = x.Id,
                        Name = x.Product.Name,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        ImageUrl = x.Product.Images[0].ImageUrl
                    }).ToList()
                };

                orderListModel.Add(orderModel);
            }

            return View(orderListModel);
        }



    }
}
