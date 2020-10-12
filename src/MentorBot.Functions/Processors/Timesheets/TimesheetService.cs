using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors.Timesheets
{
    /// <summary>A timesheet notifications service.</summary>
    public sealed class TimesheetService : ITimesheetService
    {
        private readonly IStorageService _storageService;
        private readonly ICognitiveService _cognitiveService;
        private readonly IHangoutsChatConnector _hangoutsChatConnector;

        /// <summary>Initializes a new instance of the <see cref="TimesheetService"/> class.</summary>
        public TimesheetService(
            IStorageService storageService,
            ICognitiveService cognitiveService,
            IHangoutsChatConnector hangoutsChatConnector)
        {
            _storageService = storageService;
            _cognitiveService = cognitiveService;
            _hangoutsChatConnector = hangoutsChatConnector;
        }

        /// <summary>Check a datetime against a basic cron pattern.</summary>
        public static bool CronCheck(string cron, DateTime date)
        {
            var parts = cron.Trim().Split(' ').Where(part => part.Length > 0).ToArray();
            var hourString = date.ToString("HH:mm", CultureInfo.InvariantCulture);
            var hoursCron = parts[0]
                .Split(',')
                .Where(part => part.Length > 0)
                .Select(part =>
                {
                    if (part == "*" ||
                        part == "?")
                    {
                        return "*";
                    }

                    var idx = part.IndexOf(':', StringComparison.InvariantCulture);
                    var timeStr = idx == 1 || part.Length == 1 ? ("0" + part) : part;
                    return idx == -1 ? (timeStr + ":00") : timeStr;
                })
                .ToArray();

            var hourValid = hoursCron.Any(hour => hour == "*" || hour.Equals(hourString, StringComparison.InvariantCulture));
            var daysInCurrentMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var isEom = daysInCurrentMonth == date.Day;
            var weekDayString = date.ToString("ddd", CultureInfo.InvariantCulture);
            var weekDaysCron = parts[1].Split(',');
            var weekDayValid = weekDaysCron.Any(day => day == "*" ||
                (day == "EOM" && isEom) ||
                day.Equals(weekDayString, StringComparison.InvariantCultureIgnoreCase));

            return hourValid && weekDayValid;
        }

        /// <summary>Sends the scheduled timesheet notifications asynchronous.</summary>
        public async Task SendScheduledTimesheetNotificationsAsync(DateTime scheduleDate)
        {
            var result = await _cognitiveService.GetCognitiveTextAnalysisResultAsync(
                new TextDeconstructionInformation(null, TimesheetsProperties.ProcessorSubjectName), null);

            var processor = result.CommandProcessor as ITimesheetProcessor;
            var notificationsGroups = result.PropertiesAccessor.GetPluginPropertyGroup(TimesheetsProperties.AutoNotificationsGroup);
            var customerToExcludes = result.PropertiesAccessor.GetAllPluginPropertyValues<string>(TimesheetsProperties.FilterByCustomer);
            var timesheets = new Dictionary<string, IReadOnlyList<Timesheet>>();
            foreach (var group in notificationsGroups)
            {
                var cron = group.GetValue<string>(TimesheetsProperties.AutoNotificationsCron);
                if (CronCheck(cron, scheduleDate))
                {
                    var space = group.GetValue<string>(TimesheetsProperties.AutoNotificationsSpaces);
                    var email = group.GetValue<string>(TimesheetsProperties.AutoNotificationsManagerEmail);
                    var stateName = group.GetValue<string>(TimesheetsProperties.AutoNotificationsReportName);
                    var notify = group.GetValue<bool>(TimesheetsProperties.AutoNotificationsNotify);
                    var state = Enum.Parse<TimesheetStates>(stateName, true);
                    if (string.IsNullOrEmpty(space) ||
                        string.IsNullOrEmpty(email) ||
                        state == TimesheetStates.None)
                    {
                        continue;
                    }

                    var spaces = space.Split(',').Select(sp =>
                        sp.StartsWith("spaces/", StringComparison.InvariantCultureIgnoreCase)
                        ? sp
                        : ("spaces/" + sp));

                    var key = string.Concat(stateName, "_", email);
                    foreach (var spaceName in spaces)
                    {
                        var address = _hangoutsChatConnector.GetAddressByName(spaceName);
                        if (address == null)
                        {
                            continue;
                        }

                        if (!timesheets.TryGetValue(key, out var timesheetValues))
                        {
                            timesheetValues = await processor.GetTimesheetsAsync(scheduleDate, state, email, true, customerToExcludes);
                            timesheets.Add(key, timesheetValues);
                        }

                        await processor.SendTimesheetNotificationsToUsersAsync(
                            timesheetValues,
                            email,
                            department: null,
                            notify: notify,
                            notifyByEmail: false,
                            state,
                            address,
                            _hangoutsChatConnector);
                    }
                }
            }

            await SaveGlobalStatisticsAsync(scheduleDate, customerToExcludes, result.PropertiesAccessor, processor);
        }

        private async Task SaveGlobalStatisticsAsync(
            DateTime scheduleDate,
            IReadOnlyList<string> customerToExcludes,
            IPluginPropertiesAccessor accessor,
            ITimesheetProcessor processor)
        {
            var group = accessor.GetPluginPropertyGroup(TimesheetsProperties.GlobalStatisticsGroup).FirstOrDefault();
            if (group == null)
            {
                return;
            }

            var cron = group.GetValue<string>(TimesheetsProperties.GlobalStatisticsCron);
            if (string.IsNullOrEmpty(cron) ||
                !CronCheck(cron, scheduleDate))
            {
                return;
            }

            const TimesheetStates state = TimesheetStates.Unsubmitted;
            var dateValue = scheduleDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var timeValue = scheduleDate.ToString("HH:00", CultureInfo.InvariantCulture);
            var email = group.GetValue<string>(TimesheetsProperties.GlobalStatisticsEmail);
            var timesheets = await processor.GetTimesheetsAsync(scheduleDate, state, email, true, customerToExcludes);
            var statistics = new Statistics<TimesheetStatistics[]>
            {
                Id = Guid.NewGuid().ToString(),
                Date = dateValue,
                Time = timeValue,
                Type = nameof(TimesheetStatistics),
                Data = timesheets
                    .Select(it =>
                        new TimesheetStatistics
                        {
                            UserName = it.UserName,
                            ManagerName = it.ManagerName,
                            DepartmentName = it.DepartmentName,
                            State = state,
                        })
                    .ToArray()
            };

            await _storageService.AddOrUpdateStatisticsAsync(statistics);
        }
    }
}