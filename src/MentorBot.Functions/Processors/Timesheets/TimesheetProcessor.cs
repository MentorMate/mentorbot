using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.LanguageAnalysis;

namespace MentorBot.Functions.Processors.Timesheets
{
    /// <summary>The processor that handle timesheet operations.</summary>
    public sealed class TimesheetProcessor : ITimesheetProcessor
    {
        private readonly IOpenAirConnector _openAirConnector;
        private readonly ITimesheetNotifier _timesheetNotifier;

        /// <summary>Initializes a new instance of the <see cref="TimesheetProcessor"/> class.</summary>
        public TimesheetProcessor(
            IOpenAirConnector openAirConnector,
            ITimesheetNotifier timesheetNotifier)
        {
            _openAirConnector = openAirConnector;
            _timesheetNotifier = timesheetNotifier;
        }

        /// <inheritdoc/>
        public string Name => "MentorBot.Functions.Processors.Timesheets.OpenAirProcessor";

        /// <inheritdoc/>
        public string Subject => TimesheetsProperties.ProcessorSubjectName;

        /// <summary>Gets the notification task.</summary>
        public Task NotificationTask { get; private set; }

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var notify = info.TextSentenceChunk.StartsWith("Notify", StringComparison.InvariantCultureIgnoreCase);
            var departmentValue = info
                .Entities
                .GetValueOrDefault(nameof(Department))
                ?.FirstOrDefault()
                ?.Replace(". ", ".", StringComparison.InvariantCulture);

            var customersValue = info.Entities.GetValueOrDefault(nameof(Customer), new string[0]);
            var period = OpenAirText.GetPeriod(info.Entities.GetValueOrDefault("Period")?.FirstOrDefault());
            var state = OpenAirText.GetTimesheetState(info.Entities.GetValueOrDefault(nameof(State))?.FirstOrDefault());
            var today = Contract.LocalDateTime.Date;
            var date = period == OpenAirPeriodTypes.LastWeek ? today.AddDays(-((int)today.DayOfWeek + 1)) : today;
            var senderEmail = originalChatEvent.Message.Sender.Email;
            var customersSetting = accessor.GetAllPluginPropertyValues<string>(TimesheetsProperties.FilterByCustomer);
            var customersToExclude = customersValue.Concat(customersSetting ?? new string[0]).ToArray();
            if (state == TimesheetStates.None)
            {
                return new ValueTask<ChatEventResult>(
                    new ChatEventResult("Provide a state of the time sheets, like unsubmitted or unapproved!"));
            }

            var address = new GoogleChatAddress(originalChatEvent);
            NotificationTask = SendTimesheetNotificationsAsync(
                date,
                state,
                senderEmail,
                customersToExclude,
                departmentValue,
                notify,
                false,
                true,
                address,
                responder as IHangoutsChatConnector);

            return new ValueTask<ChatEventResult>(
                new ChatEventResult(text: null));
        }

        /// <summary>Sent timesheet notifications separated by user key asynchronous.</summary>
        public async Task SendTimesheetNotificationsByKeyAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            IReadOnlyList<string> customersToExclude,
            string[] departments,
            bool notify,
            GoogleChatAddress address,
            string userKey,
            Dictionary<string, IReadOnlyList<Timesheet>> timesheets,
            IHangoutsChatConnector connector)
        {
            if (!timesheets.TryGetValue(userKey, out var timesheetValues))
            {
                timesheetValues = await GetTimesheetsAsync(date, state, email, true, customersToExclude);
                timesheets.Add(userKey, timesheetValues);
            }

            await _timesheetNotifier.SendTimesheetNotificationsToUsersAsync(
                timesheetValues,
                email,
                departments,
                notify,
                notifyByEmail: false,
                state,
                address,
                connector);
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<Timesheet>> GetTimesheetsAsync(
            DateTime dateTime,
            TimesheetStates state,
            string senderEmail,
            bool filterOutSender,
            IReadOnlyList<string> customersToExclude) =>
            _openAirConnector.GetUnsubmittedTimesheetsAsync(
                dateTime,
                Contract.LocalDateTime.Date,
                state,
                senderEmail,
                filterOutSender,
                TimesheetsProperties.UserMaxHours,
                customersToExclude);

        /// <summary>Get timesheets and notifies by message or email the users asynchronous.</summary>
        private async Task SendTimesheetNotificationsAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            IReadOnlyList<string> customersToExclude,
            string department,
            bool notify,
            bool notifyByEmail,
            bool filterOutSender,
            GoogleChatAddress address,
            IHangoutsChatConnector connector) =>
            await _timesheetNotifier.SendTimesheetNotificationsToUsersAsync(
                await GetTimesheetsAsync(date, state, email, filterOutSender, customersToExclude),
                email,
                department == null ? null : new[] { department },
                notify,
                notifyByEmail,
                state,
                address,
                connector);
    }
}
