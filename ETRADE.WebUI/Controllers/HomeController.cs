using ETRADE.Business.Abstract;
using ETRADE.Entities;
using ETRADE.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETRADE.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private IProductService _productService;
        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var products = _productService.GetAll();
            if (products==null || !products.Any())
            {
                products=new List<Product>();
            }
            return View(new ProductListModel()
            {
                Products = products
            });
        }
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
