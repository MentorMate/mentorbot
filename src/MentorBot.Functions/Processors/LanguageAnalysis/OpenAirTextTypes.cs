namespace MentorBot.Functions.Processors.LanguageAnalysis
{
    /// <summary>OpenAir text types.</summary>
    public enum OpenAirTextTypes
    {
        /// <summary>The none specified.</summary>
        None = 0,

        /// <summary>The text to notify users.</summary>
        Notify,

        /// <summary>The text for all are notified.</summary>
        AllAreNotified,

        /// <summary>The text for some are notified.</summary>
        SomeAreNotified,

        /// <summary>The text for all are done.</summary>
        AllAreDone,

        /// <summary>The text for some are done.</summary>
        SomeAreDone
    }
}
