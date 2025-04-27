using DataTables.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DhruvEnterprises.Web.Code.LIBS
{
    public static class DatatableExtensions
    {
        public static IEnumerable<IColumn> SortedColumns(this IDataTablesRequest request)
        {
            return request.Columns.Where(r => r.IsSortable && r.Sort != null);
        }

        public static IOrderedQueryable<TEntity> OrderByColumn<TEntity, TKey>(this IQueryable<TEntity> queryable
            , IColumn column
            , Expression<Func<TEntity, TKey>> keySelector)
        {
            return column.Sort.Direction == SortDirection.Ascending
                ? queryable.OrderBy(keySelector)
                : queryable.OrderByDescending(keySelector);
        }
    }
}