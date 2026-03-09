using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Pagination
{
    public interface IGenericRepository<T> where T : class
    {
        Task<PagedResult<T>> GetPagedAsync(
            IQueryable<T> query,
            PaginationRequest request,
            Expression<Func<T, bool>>? filter = null
        );
    }
}
