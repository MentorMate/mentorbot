namespace MentorBot.Functions.Processors.Timesheets
{
    /// <summary>The timesheets properties keys.</summary>
    public static class TimesheetsProperties
    {
        /// <summary>The processor name.</summary>
        public const string ProcessorSubjectName = nameof(Timesheets);

        /// <summary>The auto notifications group key.</summary>
        public const string AutoNotificationsGroup = "OpenAir.AutoNotifications";

        /// <summary>The global statistics group key.</summary>
        public const string GlobalStatisticsGroup = "OpenAir.GlobalReport";

        /// <summary>The global statistics email.</summary>
        public const string GlobalStatisticsEmail = "OpenAir.GlobalReport.Email";

        /// <summary>The global statistics cron.</summary>
        public const string GlobalStatisticsCron = "OpenAir.GlobalReport.Cron";

        /// <summary>The automatic notifications cron.</summary>
        public const string AutoNotificationsCron = "OpenAir.AutoNotifications.Cron";

        /// <summary>The automatic notifications report name.</summary>
        public const string AutoNotificationsReportName = "OpenAir.AutoNotifications.ReportName";

        /// <summary>The automatic notifications spaces.</summary>
        public const string AutoNotificationsSpaces = "OpenAir.AutoNotifications.Spaces";

        /// <summary>The automatic notifications manager email.</summary>
        public const string AutoNotificationsManagerEmail = "OpenAir.AutoNotifications.Email";

        /// <summary>The automatic notifications should notify users.</summary>
        public const string AutoNotificationsNotify = "OpenAir.AutoNotifications.Notify";

        /// <summary>The filter by customer key.</summary>
        public const string FilterByCustomer = "OpenAir.Filters.Customer";

        /// <summary>The user properties key.</summary>
        public const string UserProperties = "OpenAir.UserProperties";

        /// <summary>The user maximum hours key.</summary>
        public const string UserMaxHours = "OpenAir.User.MaxHours";
    }
}
