using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EatCleanAPI.Models
{
    public partial class OrderDetail
    {
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public short? Quantity { get; set; }
        public float? Discount { get; set; }
        public decimal? UnitPrice { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual Order Order { get; set; }
    }
}
