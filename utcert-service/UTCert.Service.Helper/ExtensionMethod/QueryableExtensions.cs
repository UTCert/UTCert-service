using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UTCert.Data.Repository.Common.ExtensionMethod
{
    public static class QueryableExtensions 
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable,
            bool condition, 
            Expression<Func<T, bool>> predicate)
        {
            return condition ? queryable.Where(predicate) : queryable;
        }

        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int maxResultCount)
        {
            return query.Skip(skipCount).Take(maxResultCount);
        }

        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string orderBy)
        {
            if (string.IsNullOrWhiteSpace(orderBy)) return source;

            var orderBys = orderBy.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var orderByClause in orderBys)
            {
                source = AddOrderBy(source, orderByClause.Trim());
            }

            return source;
        }

        private static IQueryable<T> AddOrderBy<T>(IQueryable<T> source, string orderBy)
        {
            var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var methodName = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy";
            var propertyName = parts[0];

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = BuildPropertyPathExpression(parameter, propertyName);
            var orderByLambda = Expression.Lambda(propertyAccess, parameter);

            var orderedQuery = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), propertyAccess.Type }, source.Expression, orderByLambda);
            return source.Provider.CreateQuery<T>(orderedQuery);
        }

        private static Expression BuildPropertyPathExpression(Expression root, string propertyPath)
        {
            return propertyPath.Split('.').Aggregate(root, (current, property) =>
            {
                var propInfo = current.Type.GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                return propInfo == null
                    ? throw new KeyNotFoundException($"Property '{property}' not found on type '{current.Type.Name}'.")
                    : (Expression)Expression.Property(current, propInfo);
            });
        }



    }

}
