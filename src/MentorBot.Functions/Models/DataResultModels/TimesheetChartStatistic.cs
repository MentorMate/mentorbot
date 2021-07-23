namespace MentorBot.Functions.Models.DataResultModels
{
    /// <summary>A simple timesheet unsubmitted statistic.</summary>
    public sealed class TimesheetChartStatistic
    {
        /// <summary>Gets or sets the date.</summary>
        public string Date { get; set; }

        /// <summary>Gets or sets the department.</summary>
        public string Department { get; set; }

        /// <summary>Gets or sets the count.</summary>
        public int Count { get; set; }
    }
}
