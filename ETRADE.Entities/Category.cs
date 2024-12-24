using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductCategory> ProductCategories { get; set; } //product ile categorilerin bir arada tutulduğu

        public Category()
        {
            ProductCategories = new List<ProductCategory>(); //ProductCategoryden hata almamak için boş bir liste
        }
    }
}
