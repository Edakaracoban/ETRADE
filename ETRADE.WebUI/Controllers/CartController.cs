﻿using ETRADE.Business.Abstract;
using ETRADE.Entities;
using ETRADE.WebUI.Extensions;
using ETRADE.WebUI.Identity;
using ETRADE.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace ETRADE.WebUI.Controllers
{
    public class CartController : Controller
    {
        private ICartService _cartService;
        private IProductService _productService;
        private IOrderService _orderService;
        private UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, IProductService productService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            return View(new CartModel()
            {
                CartId = cart.Id,
                CartItems = cart.CartItems.Select(x => new CartItemModel()
                {
                    CartItemId = x.Id,
                    ProductId = x.ProductId,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    ImageUrl = x.Product.Images[0].ImageUrl,
                    Quantity = x.Quantity

                }).ToList()
            });
        }
        public IActionResult AddToCart(int productId, int quantity)
        {
            _cartService.AddToCart(_userManager.GetUserId(User), productId, quantity);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteFromCart(int productId)
        {

            _cartService.DeleteFromCart(_userManager.GetUserId(User), productId);
            return RedirectToAction("Index");
        }
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            OrderModel model = new OrderModel();
            model.CartModel = new CartModel()
            {
                CartId = cart.Id,
                CartItems = cart.CartItems.Select(x => new CartItemModel()
                {
                    CartItemId = x.Id,
                    ProductId = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    ImageUrl = x.Product.Images[0].ImageUrl,
                    Quantity = x.Quantity
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Checkout(OrderModel model, string paymentMethod)
        {
            ModelState.Remove("CartModel");

            if (ModelState.IsValid) //önyüzde alanlar doğru dolduruldu mu?
            {
                var userId = _userManager.GetUserId(User);
                var cart = _cartService.GetCartByUserId(userId);

                model.CartModel = new CartModel()
                {
                    CartId = cart.Id,
                    CartItems = cart.CartItems.Select(x => new CartItemModel()
                    {
                        CartItemId = x.Id,
                        ProductId = x.Product.Id,
                        Name = x.Product.Name,
                        Price = x.Product.Price,
                        ImageUrl = x.Product.Images[0].ImageUrl,
                        Quantity = x.Quantity
                    }).ToList(),
                };

                if (paymentMethod == "credit")
                {
                    var payment = PaymentProccess(model);

                    if (payment.Result.Status == "success")
                    {
                        SaveOrder(model, payment, userId);
                        ClearCart(cart.Id.ToString());
                        TempData.Put("message", new ResultModel()
                        {
                            Title = "Order Completed",
                            Message = "Your order has been completed",
                            Css = "succes"
                        });
                    }
                    else
                    {
                        TempData.Put("message", new ResultModel()
                        {
                            Title = "Order UnCompleted",
                            Message = "Your order could not be completed",
                            Css = "danger"
                        });
                    }
                }
                else // eft için olan kısımda payment process kullanılmıyor.
                {
                    SaveOrder(model, userId);
                    ClearCart(cart.Id.ToString());
                    TempData.Put("message", new ResultModel()
                    {
                        Title = "Order Completed",
                        Message = "Your order has been completed successfully",
                        Css = "success"
                    });

                }
            }
            return View(model);

        }
        private void ClearCart(string id)
        {
            _cartService.ClearCart(id);
        }

        // EFT
        private void SaveOrder(OrderModel model, string userId)
        {
            Order order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentTypes.Eft,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversionId = Guid.NewGuid().ToString(),
                PaymentId = Guid.NewGuid().ToString(),
                OrderNote = model.OrderNote,
                OrderDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Email = model.Email,
                Phone = model.Phone,
                UserId = userId
            };

            foreach (var cartItem in model.CartModel.CartItems)
            {
                var orderItem = new Entities.OrderItem()
                {
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                    ProductId = cartItem.ProductId,
                };

                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);

        }

        // Credit Card
        private void SaveOrder(OrderModel model, Task<Payment> payment, string userId)
        {
            Order order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentTypes.CreditCard,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversionId = payment.Result.ConversationId,
                PaymentId = payment.Result.PaymentId,
                OrderNote = model.OrderNote,
                OrderDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Email = model.Email,
                Phone = model.Phone,
                UserId = userId
            };

            foreach (var cartItem in model.CartModel.CartItems)
            {
                var orderItem = new Entities.OrderItem()
                {
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                    ProductId = cartItem.ProductId,
                };

                order.OrderItems.Add(orderItem); //sepetin içindeki item değerlerini orderitem'a ekle
            }

            _orderService.Create(order);
        }


        //iyzikoyu kullanarak istek atar.//iyzipay kütüphanesi kullanılır.
        private async Task<Payment> PaymentProccess(OrderModel model)
        {
            Options options = new Options()
            {
                BaseUrl = "https://sandbox-api.iyzipay.com",
                ApiKey = "sandbox-cNnJEaoyNt0sCREL4nOq8PajTLQwWeXz",
                SecretKey = "sandbox-cmJxJfaGlVarqNV3c5ZQcMTwVNh8qswx"
            };

            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            var externalIp = IPAddress.Parse(externalIpString);

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = Guid.NewGuid().ToString();
            request.Price = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.PaidPrice = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = model.CartModel.CartId.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.PaymentChannel = PaymentChannel.WEB.ToString();

            PaymentCard paymentCard = new PaymentCard()
            {
                CardHolderName = model.CardName,
                CardNumber = model.CardNumber,
                ExpireYear = model.ExprationYear,
                ExpireMonth = model.ExprationMonth,
                Cvc = model.CVV,
                RegisterCard = 0
            };



            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer()
            {
                Id = _userManager.GetUserId(User),
                Name = model.FirstName,
                Surname = model.LastName,
                GsmNumber = model.Phone,
                Email = model.Email,
                IdentityNumber = "11111111111",
                RegistrationAddress = model.Address,
                Ip = externalIp.ToString(),
                City = model.City,
                Country = "TURKEY",
                ZipCode = "34000"


            };

            request.Buyer = buyer;

            Address address = new Address()
            {
                ContactName = model.FirstName + " " + model.LastName,
                City = model.City,
                Country = "Turkey",
                Description = model.Address,
                ZipCode = "34000"
            };

            request.BillingAddress = address;
            request.ShippingAddress = address;


            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem basketItem;

            foreach (var cartItem in model.CartModel.CartItems)
            {
                basketItem = new BasketItem()
                {
                    Id = cartItem.ProductId.ToString(),
                    Name = cartItem.Name,
                    Category1 = _productService.GetProductDetail(cartItem.ProductId).ProductCategories.FirstOrDefault().CategoryId.ToString(),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (cartItem.Price * cartItem.Quantity).ToString().Split(",")[0]
                };

                basketItems.Add(basketItem);
            }

            request.BasketItems = basketItems;

            Payment payment = await Payment.Create(request, options);

            return payment;
        }

    }
}
 