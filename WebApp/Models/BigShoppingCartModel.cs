using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class BigShoppingCartModel
    {
        public ShoppingCart ShoppingCart { get; set; }

        public Guid productId { get; set; }

        public int quantity { get; set; }
    }
}
