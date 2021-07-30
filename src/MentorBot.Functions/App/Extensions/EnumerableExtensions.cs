using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>Extensions related to enumerable types.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Any string in collection.</summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        public static bool AnyStringInCollection<T>(
            this IEnumerable<T> entities,
            IEnumerable<string> collection,
            Func<T, string> selector,
            StringComparison comparison) =>
            entities.Select(selector).Any(value => collection.Any(it => it.Equals(value, comparison)));

        /// <summary>Applies the specified action to every item in the collection.</summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> entities, Action<T> action)
        {
            foreach (var entity in entities)
            {
                action(entity);
                yield return entity;
            }
        }
    }
}
