using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Abstract
{
    public interface IProductDal :IRepository<Product>
    {
        int GetCountByCategory(string category);
        Product GetProductDetails(int id);
        List<Product> GetProductsByCategory(string category,int page,int pageSize); //pagination çok data gelince kull.
        void Update(Product entity, int[] categoryIds);

    }
}
