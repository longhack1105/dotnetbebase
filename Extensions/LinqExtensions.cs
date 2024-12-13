using System.Linq.Expressions;

namespace DotnetBeBase.Extensions
{
    public static class LinqExtensions
    {
        public static IOrderedEnumerable<T> OrderByDescendingIf<T, TKey>(
            this IEnumerable<T> source,
            Func<T, bool> condition,
            Func<T, TKey> keySelector)
        {
            return source.OrderByDescending(condition).ThenByDescending(keySelector);
        }

        public static IOrderedQueryable<T> OrderByDescendingIf<T, TKey>(
            this IQueryable<T> source,
            Expression<Func<T, bool>> condition,
            Expression<Func<T, TKey>> keySelector)
        {
            return source.OrderByDescending(condition).ThenByDescending(keySelector);
        }

        public static IOrderedEnumerable<T> ThenByDescendingIf<T, TKey>(
            this IOrderedEnumerable<T> source,
            Func<T, bool> condition,
            Func<T, TKey> keySelector)
        {
            return condition(source.FirstOrDefault())
                ? source.ThenByDescending(keySelector)
                : source.ThenBy(keySelector);
        }

        public static IOrderedQueryable<T> ThenByDescendingIf<T, TKey>(
            this IOrderedQueryable<T> source,
            Expression<Func<T, bool>> condition,
            Expression<Func<T, TKey>> keySelector)
        {
            var firstItemCondition = Expression.Lambda<Func<T, bool>>(condition.Body, condition.Parameters).Compile();
            return firstItemCondition(source.FirstOrDefault())
                ? source.ThenByDescending(keySelector)
                : source.ThenBy(keySelector);
        }
    }
}
