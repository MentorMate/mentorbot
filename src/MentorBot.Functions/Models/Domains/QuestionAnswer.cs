namespace MentorBot.Functions.Models.Domains
{
    /// <summary> The entity we get from the google sheets. </summary>
    public sealed class QuestionAnswer
    {
        /// <summary> Gets or sets the id. </summary>
        public int Id { get; set; }

        /// <summary> Gets or sets the index. </summary>
        public int Index { get; set; }

        /// <summary> Gets or sets the contet. </summary>
        public string Content { get; set; }

        /// <summary> Gets or sets the type. </summary>
        public QuestionAnswerType Type { get; set; }
    }
}
