using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EatCleanAPI.Models
{
    public partial class Product
    {
        public Product()
        {
            MenuDetails = new HashSet<MenuDetail>();
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsOnOrder { get; set; }
        public bool? Discontinued { get; set; }
        public int? Calories { get; set; }
        public float? Protein { get; set; }
        public float? Carb { get; set; }
        public float? Fat { get; set; }
        public string ProductDescription { get; set; }
        public string Images { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<MenuDetail> MenuDetails { get; set; }
    }
}
