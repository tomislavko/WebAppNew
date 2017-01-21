﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApp.Data;
using WebApp.Interfaces;
using WebApp.Models;
using EntityState = System.Data.Entity.EntityState;

namespace WebApp.Logic
{
    public class WebAppSqlRepository : IWebAppRepository, IShoppingCart
    {
        private readonly WebAppDbContext _context;


        public WebAppSqlRepository(WebAppDbContext context)
        {
            _context = context;
        }

        

        #region Product repository implementation


        public void AddProduct(Product product)
        {
            if ( _context.Products.Any(s => s.ProductId == product.ProductId))
            {
                throw new DuplicateItemExeption();
            }
            _context.Products.Add(product);
            _context.SaveChanges();
        }
            

        public bool RemoveProduct(Guid productId)
        {
            var prod = GetProduct(productId);
            if (prod == null)
            {
                return false;
            }
            _context.Products.Remove(prod);
            _context.SaveChanges();
            return true;
        }

        public Product GetProduct(Guid productId)
        {
            return _context.Products.SingleOrDefault(s => s.ProductId == productId);
        }

        public bool TakeProduct(Guid productId, int quantity)
        {
            var prod = GetProduct(productId);
            if (prod == null)
            {
                return false;
            }
            if (prod.IsUnlimited)
            {
                return true;
            }
            if (prod.Quantity < quantity)
            {
                return false;
            }
            prod.Quantity -= quantity;
            _context.SaveChanges();
            return true;
        }

        public void Update(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public List<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public  int GetProductQuantity(Guid productId)
        {
            var prod = GetProduct(productId);
            if (prod == null)
            {
                return 0;
            }
            if (prod.IsUnlimited)
            {
                return -1;
            }
            return prod.Quantity;
        }

        public List<Product> GetFiltered(Func<Product, bool> filterFunction)
        {
            return _context.Products.Where(filterFunction).ToList();
        }


        #endregion



        #region Shopping cart repository implementation



        public void AddCart(ShoppingCart shoppingCart)
        {
            if (shoppingCart == null)
            {
                throw new NullReferenceException("Argument must be defined!");
            }
           
            if (_context.ShoppingCarts.Any(s => s.CartId == shoppingCart.CartId))
            {
                throw new DuplicateItemExeption();
            }
            _context.ShoppingCarts.Add(shoppingCart);
            _context.SaveChanges();
        }

        public bool RemoveCart(Guid cartId)
        {
            var cart = GetCart(cartId);
            if (cart == null)
            {
                return false;
            }
            _context.ShoppingCarts.Remove(cart);
            _context.SaveChanges();
            return true;
        }

        public ShoppingCart GetCart(Guid cartId)
        {
            return _context.ShoppingCarts.SingleOrDefault(s => s.CartId == cartId);
        }

        public List<ShoppingCart> GetUserCarts(User user)
        {
            return _context.ShoppingCarts.Where(s => s.User.Equals(user)).ToList();
        }

        public List<ShoppingCart> GetFiltered(Func<ShoppingCart, bool> filterFunction)
        {
            return _context.ShoppingCarts.Where(filterFunction).ToList();
        }

        public List<Product> GetMostPopularProducts(int n)
        {
            /*
           List<ShoppingCart> carts = _context.ShoppingCarts.ToList();
           List<Product> products = new List<Product>();
           foreach (ShoppingCart shoppingCart in carts)
           {
               products.AddRange(shoppingCart.CartedProducts);
           }
           */
            throw new NotImplementedException();
        }

        // maybe unnecessary
        public bool CreateOrder(Order order)
        {
            if (order == null || _context.Orders
                .Any(s => s.ShopppingCartId == order.ShopppingCartId || s.OrderId == order.OrderId))
            {
                return false;
            }
            _context.Orders.Add(order);
            _context.SaveChanges();
            return true;
        }


        public List<Product> GetNProducts(int n)
        {
            return _context.Products.Take(n).ToList();
        }


        #endregion



        #region Implementation of shopping cart



        public ShoppingCart GetActiveCart(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            return _context.ShoppingCarts.SingleOrDefault(cart => cart.User.Email == user.Email
                                                                  && cart.User.UserName == user.UserName
                                                                  && cart.User.Id == user.Id
                                                                  && !cart.IsCompleted);
        }

        public void MarkAsCompleted(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }
            
            var cart = GetActiveCart(user);
            if (cart != null)
            {
                MarkAsCompleted(cart.CartId);
            }
            
        }

        public void MarkAsCompleted(Guid cartId)
        {
            var cart = _context.ShoppingCarts.SingleOrDefault(s => s.CartId == cartId);
            if (cart != null && !cart.IsCompleted)
            {
                cart.IsCompleted = true;
                cart.DateRequested = DateTime.UtcNow;
                _context.SaveChanges();
                //UpdateCart(cart);
            }
            
        }

        public void UpdateCart(ShoppingCart shoppingCart)
        {
            _context.Entry(shoppingCart).State = EntityState.Modified;
            _context.SaveChanges();
        }


        public void AddToCart(User user, Product product, int quantity)
        {
            if (quantity <= 0 || product == null)
            {
                throw new ArgumentException();
            }
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                cart = new ShoppingCart(user);
                cart.CartedProducts.Add(product);
                AddCart(cart);
                return;
            }
            var prod = cart.CartedProducts.SingleOrDefault(s => s.ProductId == product.ProductId);
            if (prod == null)
            {
                cart.CartedProducts.Add(new Product(product, quantity));
            }
            else
            {
                prod.Quantity += quantity;
            }
            _context.SaveChanges();
            //UpdateCart(cart);
        }


        public bool RemoveFromCart(User user, Guid productId, int quantity, bool all)
        {
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                return false;
            }
            if (quantity <= 0)
            {
                throw new ArgumentException();
            }

            var prod = cart.CartedProducts.SingleOrDefault(s => s.ProductId == productId);
            if (prod == null)
            {
                return false;
            }
            if (all || prod.Quantity <= quantity)
            {
                cart.CartedProducts.Remove(prod);
                    _context.SaveChanges();
                return true;
            }
            prod.Quantity -= quantity;
                _context.SaveChanges();
            //UpdateCart(cart);
            return true;
        }

        public void EmptyCart(User user)
        {
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                return;
            }
            cart.CartedProducts = new List<Product>();
            _context.SaveChanges();
            //UpdateCart(cart);
        }

        public List<Product> GetCartItems(User user)
        {
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                return new List<Product>();
            }
            return cart.CartedProducts.ToList();
        }

        public int GetCount(User user)
        {
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                return 0;
            }
            return cart.CartedProducts.Count;
        }

        public Guid? GetCartId(User user)
        {
            var cart = GetActiveCart(user);
            return cart?.CartId;
        }


        public decimal GetTotalPrice(User user)
        {
            var cart = GetActiveCart(user);
            if (cart == null)
            {
                return 0;
            }
            decimal price = 0;
            foreach (Product cartedProduct in cart.CartedProducts)
            {
                price += cartedProduct.ProductPrice;
            }
            return price;
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUser(ApplicationUser user)
        {
            if (user == null)
            {
                return null;
            }
            return _context.Users.Find(user.Id);

        }

      


        #endregion

    }
}
