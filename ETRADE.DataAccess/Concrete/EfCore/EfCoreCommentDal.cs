using ETRADE.DataAccess.Abstract;
using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore
{
    public class EfCoreCommentDal :EfCoreGenericRepository<Comment,DataContext>,ICommentDal
    {
    }
}
