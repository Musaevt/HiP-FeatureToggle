using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    internal static class LinqExtensions
    {
        public static ISet<T> ToSet<T>(this IEnumerable<T> collection) => new HashSet<T>(collection);

        public static IQueryable<T> IncludeIf<T>(this IQueryable<T> query, bool include, string navigationPropertyPath)
            where T : class
        {
            return include ? query.Include(navigationPropertyPath) : query;
        }
    }
}
