using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Pagination
{
    public class GenericRepository<T> : IGenericRepository<T>
    where T : class
    {
        public async Task<PagedResult<T>> GetPagedAsync(
            IQueryable<T> query,
            PaginationRequest request,
            Expression<Func<T, bool>>? filter = null)
        {
            if (filter != null)
                query = query.Where(filter);

            var totalRecords = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = ApplySorting(query, request.SortBy, request.SortOrder);
            }

            var skip = (request.PageNumber - 1) * request.PageSize;

            var items = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new PagedResult<T>
            {
                Items = items,
                Pagination = new PaginationMeta
                {
                    TotalRecords = totalRecords,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                }

            };
        }

        private static IQueryable<T> ApplySorting(
            IQueryable<T> query,
            string sortBy,
            string? sortOrder)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(param, sortBy);
            var lambda = Expression.Lambda(property, param);

            var method = sortOrder?.ToLower() == "desc"
                ? "OrderByDescending"
                : "OrderBy";

            var call = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), property.Type],
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(call);
        }
    }

}
