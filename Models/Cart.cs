using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CafateriaSystem.Models
{
    public class Cart
    {
        public Menu Menu { get; set; }
        public int Quantity { get; set; }
        public Cart(Menu menu, int quantity)
        {
            Menu = menu;
            Quantity = quantity;
        }
    }
}