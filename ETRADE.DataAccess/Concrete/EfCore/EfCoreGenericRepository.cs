using ETRADE.DataAccess.Abstract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore //Dataacces katmanı veri tabanına bağlanıp CRUD(create-read-update-delete) işlemlerini yapar
{
    public class EfCoreGenericRepository<T, TContext> : IRepository<T> where T : class where TContext : DbContext, new()
    {
        public void Create(T entity)
        {
            using (var context = new TContext())
            {
                context.Set<T>().Add(entity);
                context.SaveChanges(); //SaveChanges;insert,update,delete sorgularını oluşturup bir transaction
                //eşliğinde veritabanına gönderip execute eden fonksiyondur.
            }
        }

        public virtual void Delete(T entity)
        {
            using (var context = new TContext())
            {
                context.Set<T>().Remove(entity);
                context.SaveChanges();
            }
        }

        public virtual List<T> GetAll(Expression<Func<T, bool>> filter = null) //polymorphism
        {
            using (var context = new TContext())
            {
                return filter == null ? context.Set<T>().ToList() : context.Set<T>().Where(filter).ToList();
            }
        }

        public T GetById(int id)
        {
            using (var context = new TContext())
            {
                return context.Set<T>().Find(id);
            }
        }
        // changetracker değişiklikleri takip eder ve değişiklikleri veritabanına yansıtır
        public T GetOne(Expression<Func<T, bool>> filter = null)
        {
            using (var context = new TContext())
            {
                return context.Set<T>().Where(filter).FirstOrDefault();//verdiğimiz şarta uygun verilerin arasından ilkini getirir
            }
        }

        public virtual void Update(T entity)
        {
            using (var context = new TContext())
            {
                context.Entry(entity).State= EntityState.Modified;
                context.SaveChanges();
            }
        }
    }
}
