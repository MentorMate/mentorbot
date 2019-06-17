// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>Extensions related to enumerable types.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Anies the string in collection.</summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        public static bool AnyStringInCollection<T>(this IEnumerable<T> entities, IEnumerable<string> collection, Func<T, string> selector, StringComparison comparison) =>
            entities.Select(selector).Any(value => collection.Any(it => it.Equals(value, comparison)));
    }
}
