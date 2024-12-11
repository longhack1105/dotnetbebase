using System.Runtime.Serialization;

namespace ChatApp.Extensions
{
    public static class BasicExtension
    {
        public static PagedList<T> TakePage<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            return PagedList<T>.ToPagedList(source, pageNumber, pageSize);
        }

        public static async Task<PagedList<T>> TakePage<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            return await PagedList<T>.ToPagedList(source, pageNumber, pageSize);
        }
    }
}
