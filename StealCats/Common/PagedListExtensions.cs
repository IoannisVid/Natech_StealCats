using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace StealTheCats.Common
{
    public static class PagedListExtensions
    {
        //public static async Task<PagedList<TDto>> CreateAsync<T, TDto>(IQueryable<T> source, int pageNumber, int pageSize, IConfigurationProvider mapperConfig)
        //{
        //    var count = await source.CountAsync();

        //    var items = await source
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ProjectTo<TDto>(mapperConfig)
        //        .ToListAsync();

        //    return new PagedList<TDto>(items, count, pageNumber, pageSize);
        //}
    }
}
