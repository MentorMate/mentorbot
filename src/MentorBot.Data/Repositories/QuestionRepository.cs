// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Core.Abstract.Repositories;
using MentorBot.Data.Common;
using MentorBot.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace MentorBot.Data.Repositories
{
    /// <summary>
    /// The basic implementation of a <see cref="IQuestionRepository"/>.
    /// </summary>
    /// <seealso cref="DbRepository{Question}"/>
    /// <seealso cref="IQuestionRepository"/>
    public class QuestionRepository : DbRepository<Question>, IQuestionRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionRepository"/> class.
        /// </summary>
        public QuestionRepository(DbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public Task AddAsync(Question question) => AddEntityAsync(question);
    }
}
