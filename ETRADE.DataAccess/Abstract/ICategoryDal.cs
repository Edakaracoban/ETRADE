using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Abstract
{
    public interface ICategoryDal:IRepository<Category>
    {
        void DeleteFromCategory(int categoryId,int productId);
        Category GetByIdWithProducts(int id); //productidye göre kategorileri döndürecek
    }
}
