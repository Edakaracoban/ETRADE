﻿using ETRADE.DataAccess.Abstract;
using ETRADE.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore
{
    public class EfCoreCategoryDal : EfCoreGenericRepository<Category, DataContext>, ICategoryDal
    {
        public void DeleteFromCategory(int categoryId, int productId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from ProductCategory where ProductId=@p1 and CategoryId=@p0";
                context.Database.ExecuteSqlRaw(cmd, categoryId, productId);
            }

        }

        public Category GetByIdWithProducts(int id)
        {
            using (var context = new DataContext())
            {
                return context.Categories
                       .Where(i => i.Id == id) //gelen id ile böyle bir kategori idsi var mı?
                       .Include(i => i.ProductCategories)
                       .ThenInclude(i => i.Product)
                       .ThenInclude(i => i.Images)
                       .FirstOrDefault();// Sorgu neticesinde elde edilen verilerden ilkini getirir eğer veri yoksa null döner.
            }
        }
        public override void Delete(Category entity)
        {
            using (var context = new DataContext())
            {
                context.Categories.Remove(entity);
                context.SaveChanges();
            }
        }
    }
}
