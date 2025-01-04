using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Business.Abstract
{
    public interface ICategoryService
    {
        Category GetById(int id);
        Category GetByWithProducts(int id); // Herhangi bir producta tıkladığında o producta ait kategoriyi getir.
        List<Category> GetAll();
        void Create(Category entity);
        void Update(Category entity);
        void Delete(Category entity);
        void DeleteFromCategory(int categoryId, int productId);
    }
}
