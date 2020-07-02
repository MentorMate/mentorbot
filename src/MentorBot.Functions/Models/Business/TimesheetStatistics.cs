namespace MentorBot.Functions.Models.Business
{
    /// <summary>A timesheet statistic data for user not submitted or approved.</summary>
    public sealed class TimesheetStatistics
    {
        /// <summary>Gets or sets the name of the user.</summary>
        public string UserName { get; set; }

        /// <summary>Gets or sets the name of the department.</summary>
        public string DepartmentName { get; set; }

        /// <summary>Gets or sets the name of the manager.</summary>
        public string ManagerName { get; set; }

        /// <summary>Gets or sets the state.</summary>
        public TimesheetStates State { get; set; }
    }
}
