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

        /// <summary>Sends the scheduled timesheet notifications asynchronous.</summary>
        public async Task SendScheduledTimesheetNotificationsAsync()
        {
            var now = DateTime.Now;
            var dateValue = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var timeValue = now.ToString("HH:00", CultureInfo.InvariantCulture);
            var result = await _cognitiveService.GetCognitiveTextAnalysisResultAsync(
                new TextDeconstructionInformation(null, TimesheetsProperties.ProcessorSubjectName), null);

            var processor = result.CommandProcessor as ITimesheetProcessor;
            var notificationsGroups = result.PropertiesAccessor.GetPluginPropertyGroup(TimesheetsProperties.AutoNotificationsGroup);
            var customerToExcludes = result.PropertiesAccessor.GetAllPluginPropertyValues<string>(TimesheetsProperties.FilterByCustomer);
            var timesheets = new Dictionary<string, IReadOnlyList<Timesheet>>();
            foreach (var group in notificationsGroups)
            {
                var cron = group.GetValue<string>(TimesheetsProperties.AutoNotificationsCron);
                if (CronCheck(cron, now))
                {
                    var space = group.GetValue<string>(TimesheetsProperties.AutoNotificationsSpaces);
                    var email = group.GetValue<string>(TimesheetsProperties.AutoNotificationsManagerEmail);
                    var stateName = group.GetValue<string>(TimesheetsProperties.AutoNotificationsReportName);
                    var state = Enum.Parse<TimesheetStates>(stateName, true);
                    if (string.IsNullOrEmpty(space) ||
                        string.IsNullOrEmpty(email) ||
                        state == TimesheetStates.None)
                    {
                        continue;
                    }

                    var spaces = space.Split(',');
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
                            timesheetValues = await processor.GetTimesheetsAsync(now, state, email, true, customerToExcludes);
                            timesheets.Add(key, timesheetValues);

                            var statistics = new Statistics<TimesheetStatistics[]>
                            {
                                Id = Guid.NewGuid().ToString(),
                                Date = dateValue,
                                Time = timeValue,
                                Data = timesheetValues
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

                        await processor.SendTimesheetNotificationsToUsersAsync(
                            timesheetValues,
                            email,
                            department: null,
                            notify: false,
                            notifyByEmail: false,
                            state,
                            address,
                            _hangoutsChatConnector);
                    }
                }
            }
        }

        private static bool CronCheck(string cron, DateTime date)
        {
            var parts = cron.Trim().Split(' ').Where(part => part.Length > 0).ToArray();
            if (parts[1] != "*" &&
                parts[1].ToUpperInvariant() != date.ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant())
            {
                return false;
            }

            if (parts[0] != "*" &&
                parts[0] != date.Hour.ToString(CultureInfo.InvariantCulture))
            {
                return false;
            }

            return true;
        }
    }
}
