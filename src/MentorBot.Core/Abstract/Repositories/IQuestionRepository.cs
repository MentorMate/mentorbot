// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;
using MentorBot.Data.Common;
using MentorBot.Data.Models;

namespace MentorBot.Core.Abstract.Repositories
{
    /// <summary>
    /// Repository for accessing data related to <see cref="Question"/>.
    /// </summary>
    public interface IQuestionRepository : IDbRepository<Question>
    {
        /// <summary>
        /// Adds new question record in <see cref="Question"/> table asynchronously.
        /// </summary>
        Task AddAsync(Question question);
    }
}
