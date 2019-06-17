namespace MentorBot.Functions.Models.Business
{
    /// <summary>A enumeration of manager levels.</summary>
    public enum ManagerLevels
    {
        /// <summary>The none specified.</summary>
        None = 0,

        /// <summary>The department's manager.</summary>
        Department,

        /// <summary>The line manager.</summary>
        Manager,

        /// <summary>The manager's manager.</summary>
        ManagerManager
    }
}
