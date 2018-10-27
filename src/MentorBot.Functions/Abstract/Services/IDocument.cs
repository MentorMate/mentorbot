// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Document operations.</summary>
    /// <typeparam name="T">The document resource type.</typeparam>
    public interface IDocument<T>
    {
        /// <summary>Execute SQL expression on collection.</summary>
        /// <param name="sqlExpression">The SQL expression.</param>
        IReadOnlyList<T> Query(string sqlExpression);

        /// <summary>Add an document object to the collection.</summary>
        /// <param name="item">The new object.</param>
        Task AddAsync(T item);
    }
}
