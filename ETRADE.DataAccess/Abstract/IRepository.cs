using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Abstract
{
    public interface IRepository<T> //T entitye karşılık gelmekte
    {
        T GetById(int id);
        T GetOne(Expression<Func<T, bool>> filter=null); //filter gelmesse default parametresi null olacak//sql sorgusu
        List<T> GetAll(Expression<Func<T,bool>> filter=null); //sorgu almassa null olucak
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
