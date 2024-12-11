using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Utils
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(count / (double)(pageSize));
            AddRange(items);
        }

        /// <summary>
        /// This must passing IEnumerable
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var page_number = pageNumber <= 0 ? 1 : pageNumber;
            var items_per_page = pageSize <= 0 ? count : pageSize;
            var items = source.Skip((page_number - 1) * items_per_page).Take(items_per_page).ToList();
            var result = new PagedList<T>(items, count, page_number, items_per_page);
            return result;
        }

        /// <summary>
        /// This must passing IQuery
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PagedList<T>> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var page_number = pageNumber <= 0 ? 1 : pageNumber;
            var items_per_page = pageSize <= 0 ? count : pageSize;
            var items = await source.Skip((page_number - 1) * items_per_page).Take(items_per_page).ToListAsync();
            return new PagedList<T>(items, count, page_number, items_per_page);
        }
    }
}
