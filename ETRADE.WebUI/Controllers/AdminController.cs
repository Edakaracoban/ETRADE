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
        private IWebHostEnvironment _webHostEnvironment;

        public AdminController(IProductService productService, ICategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        //productları listeleyecek
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
            ViewBag.Category = category.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
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

                    ViewBag.Category = _categoryService.GetAll().Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View(model);
                }

                var entity = new Product()
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price

                };

                if (files.Count > 0 && files != null)
                {
                    if (files.Count < 4)
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
                }

                entity.ProductCategories = new List<ProductCategory>()
                {
                    new ProductCategory { CategoryId = int.Parse(model.CategoryId), ProductId = entity.Id }
                };
                _productService.Create(entity);
                return RedirectToAction("ProductList");
            }
            ViewBag.Category = _categoryService.GetAll().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(model);
        }
        // Not : Önyüzde 4 resmi değiştirmeden kayıt edersek hata vermiyor.4 resmin 4 ünü de değiştirmessek yani yalnızca 1 veya 2 resim değişikliği yaparsak hata veriyor.
        public IActionResult EditProduct(int id)
        {
            if (id == 0) // null yerine 0 kontrolü
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
                Images = entity.Images.ToList(), // entitydeki resimler birden fazla olduğu için listeledik.
                SelectedCategories = entity.ProductCategories.Select(i => i.Category).ToList()
            };

            ViewBag.Categories = _categoryService.GetAll();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductModel model, List<IFormFile> files, int[] categoryIds)
        {
            var entity = _productService.GetById(model.Id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Price = model.Price;

            if (files != null && files.Count > 0)
            {
                using (var context = new DataContext())
                {
                    var existingImages = context.Images.Where(i => i.ProductId == entity.Id).ToList();
                    context.Images.RemoveRange(existingImages); // Eski resimleri sil
                    await context.SaveChangesAsync();
                }
                foreach (var file in files)
                {

                    var image = new Image
                    {
                        ImageUrl = file.FileName,
                        ProductId = entity.Id
                    };

                    entity.Images.Add(image);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", uniqueFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            _productService.Update(entity, categoryIds);

            // Yeni resimleri veritabanına ekle
            using (var context = new DataContext())
            {
                context.Images.AddRange(entity.Images);
                await context.SaveChangesAsync();
            }
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












    }

}
