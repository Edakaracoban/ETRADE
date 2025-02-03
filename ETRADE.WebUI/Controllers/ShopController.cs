using ETRADE.Business.Abstract;
using ETRADE.Entities;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

namespace ETRADE.WebUI.Controllers
{
    public class ShopController : Controller
    {
        private IProductService _productService;
        public ShopController(IProductService productService)
        {
            _productService = productService;
        }

        [Route("products/{category?}")]
        public IActionResult List(string category, int page = 1)
        {
            const int pageSize = 5;
            var products = new ProductListModel()
            {
                PageInfo = new PageInfo()
                {

                    TotalItems = _productService.GetCountByCategory(category),
                    ItemsPerPage = pageSize,
                    CurrentCategory = category,
                    CurrentPage = page
                },
                Products = _productService.GetProductByCategory(category, page, pageSize)
            };

            return View(products);
        }

        public IActionResult Details(int? id)//int? ifadesi id parametresinin null olabilmesini sağlar.
        {
            if (id==null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            Product product = _productService.GetProductDetail(id.Value); //id null olamadığı için value özelliği kullanılır.
            if (product == null)
            {
                return NotFound();
            }
            return View(new ProductDetailsModel()
            {
                Product = product,
                Categories=product.ProductCategories.Select(c => c.Category).ToList(),
                Comments=product.Comments
            });
        }
    }
}
