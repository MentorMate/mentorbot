using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary> Either a question or an answer. </summary>
    [Storable("QuestionsAnswers")]
    public sealed class QuestionAnswer
    {
        /// <summary> Gets or sets the id. </summary>
        [RowKey]
        public string Id { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary>Gets the partition key for this record.</summary>
        [PartitionKey]
        public string PartitionKey { get; } = "System";

        /// <summary> Gets or sets the parent id. </summary>
        public string QuestionId { get; set; }

        /// <summary> Gets or sets the index. </summary>
        public int Index { get; set; }

        /// <summary> Gets or sets the content. </summary>
        public string Content { get; set; }

        /// <summary> Gets or sets the type. </summary>
        public QuestionAnswerType Type { get; set; }

        /// <summary> Gets or sets the subquestions. </summary>
        public QuestionAnswer[] SubQuestions { get; set; }
    }
}
