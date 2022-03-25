using System.Collections.Generic;

namespace MentorBot.Functions.Models.ViewModels
{
    /// <summary> Either a question or an answer. </summary>
    public sealed class QuestionAnswerViewModel
    {
        /// <summary> Gets or sets the id. </summary>
        public string Id { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary> Gets or sets the parent id. </summary>
        public Dictionary<string, string> Parents { get; set; } = new Dictionary<string, string>();

        /// <summary> Gets or sets the index. </summary>
        public int Index { get; set; }

        /// <summary> Gets or sets the title. </summary>
        public string Title { get; set; }

        /// <summary> Gets or sets the mentormater type. </summary>
        public string[] RequiredTraits { get; set; }

        /// <summary> Gets or sets the mentormater type. </summary>
        public string[] AcquireTraits { get; set; }

        /// <summary> Gets or sets the content. </summary>
        public string Content { get; set; }

        /// <summary> Gets or sets a value indicating whether the entity is an answer. </summary>
        public bool IsAnswer { get; set; }

        /// <summary> Gets or sets a value indicating whether the entity is edited. </summary>
        public bool IsEdited { get; set; }

        /// <summary> Gets or sets the subquestions. </summary>
        public QuestionAnswerViewModel[] SubQuestions { get; set; }
    }
}
