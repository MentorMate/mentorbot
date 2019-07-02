namespace MentorBot.Functions.Models.Business
{
    /// <summary>A manager information.</summary>
    public sealed class ManagerReference
    {
        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the level.</summary>
        public ManagerLevels Level { get; set; }
    }
}
