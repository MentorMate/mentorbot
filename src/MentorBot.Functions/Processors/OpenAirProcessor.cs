// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.LanguageAnalysis;

namespace MentorBot.Functions.Processors
{
    /// <summary>The processor that handle timesheet operations.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class OpenAirProcessor : ICommandProcessor, ITimesheetProcessor
    {
        private readonly IOpenAirConnector _openAirConnector;
        private readonly IStorageService _storageService;
        private readonly IMailService _mailService;

        /// <summary>Initializes a new instance of the <see cref="OpenAirProcessor"/> class.</summary>
        public OpenAirProcessor(
            IOpenAirConnector openAirConnector,
            IStorageService storageService,
            IMailService mailService)
        {
            _openAirConnector = openAirConnector;
            _storageService = storageService;
            _mailService = mailService;
        }

        /// <inheritdoc/>
        public string Subject => "Timesheets";

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IReadOnlyDictionary<string, string> settings)
        {
            var notify = info.TextSentanceChunk.StartsWith("Notify", StringComparison.InvariantCultureIgnoreCase);
            var departmentValue = info.Entities.GetValueOrDefault(nameof(Department))?.FirstOrDefault()?.Replace(". ", ".", StringComparison.InvariantCulture);
            var customersValue = info.Entities.GetValueOrDefault(nameof(Customer), new string[0]);
            var period = OpenAirText.GetPeriod(info.Entities.GetValueOrDefault("Period")?.FirstOrDefault());
            var state = OpenAirText.GetTimesheetState(info.Entities.GetValueOrDefault("State")?.FirstOrDefault());
            var today = DateTime.Today;
            var date = period == OpenAirPeriodTypes.LastWeek ? today.AddDays(-((int)today.DayOfWeek + 1)) : today;
            var senderEmail = originalChatEvent.Message.Sender.Email;
            var customersSetting = settings.GetAsArray(Default.DefaultExcludedClientKey);
            var customersToExclude = customersValue.Concat(customersSetting ?? new string[0]).ToArray();
            var notifyByEmail = settings.GetValueOrDefault(Default.NotifyByEmailKey)?.ToLowerInvariant() == "true";
            if (state == TimesheetStates.None)
            {
                return new ValueTask<ChatEventResult>(
                    new ChatEventResult("Provide a state of the time sheets, like unsubmitted or unapproved!"));
            }

            NotifyAsync(date, state, senderEmail, customersToExclude, departmentValue, notify, notifyByEmail, new GoogleChatAddress(originalChatEvent), responder as IHangoutsChatConnector);

            return new ValueTask<ChatEventResult>(
                new ChatEventResult(text: null));
        }

        /// <summary>Get timesheets and notifies by message or email the users asynchronous.</summary>
        public Task NotifyAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            string[] customersToExclude,
            string department,
            bool notify,
            bool notifyByEmail,
            GoogleChatAddress address,
            IHangoutsChatConnector connector) =>
            _openAirConnector.GetUnsubmittedTimesheetsAsync(date, state, email, customersToExclude)
                .ContinueWith(task => ProcessNotifyAsync(
                    task.Result,
                    email,
                    department,
                    notify,
                    notifyByEmail,
                    state,
                    address,
                    connector));

        private static string GetCardText(IReadOnlyList<Timesheet> timesheets, IReadOnlyList<string> notifiedUserList) =>
            string.Join(string.Empty, timesheets.Where(it => !notifiedUserList.Contains(it.UserName))
                .Select(it => $"<b>{it.UserName}:</b> {it.Total} <i>({it.DepartmentName}, {it.ManagerName})</i><br>"));

        /// <summary>Processes the specified timesheets.</summary>
        private async Task ProcessNotifyAsync(
            IReadOnlyList<Timesheet> timesheets,
            string email,
            string department,
            bool notify,
            bool notifyByEmail,
            TimesheetStates state,
            GoogleChatAddress address,
            IHangoutsChatConnector connector)
        {
            string text;
            var filteredTimesheet = timesheets
                .Where(it =>
                    department == null ||
                    department.Equals(it.DepartmentName, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var notifiedUserList = new List<string>();
            if (filteredTimesheet.Length == 0)
            {
                text = OpenAirText.GetText(state, OpenAirTextTypes.AllAreDone);
            }
            else if (notify && filteredTimesheet.Length > 0)
            {
                notifiedUserList.AddRange(
                    await NotifyUsersOverChatAsync(connector, state, filteredTimesheet));

                if (notifyByEmail)
                {
                    var textMessage = OpenAirText.GetText(state, OpenAirTextTypes.Notify);
                    var emails = filteredTimesheet
                        .Where(it => !notifiedUserList.Contains(it.UserName))
                        .Apply(it => notifiedUserList.Add(it.UserName))
                        .Select(it => it.UserEmail)
                        .Distinct()
                        .ToArray();

                    await _mailService.SendMailAsync("Timesheet is pending", textMessage, emails);
                }

                text = notifiedUserList.Count == filteredTimesheet.Length ?
                    string.Format(CultureInfo.InvariantCulture, OpenAirText.GetText(state, OpenAirTextTypes.AllAreNotified), notifiedUserList.Count) :
                    OpenAirText.GetText(state, OpenAirTextTypes.SomeAreNotified) + GetCardText(filteredTimesheet, notifiedUserList);
            }
            else
            {
                text = OpenAirText.GetText(state, OpenAirTextTypes.SomeAreDone) + GetCardText(filteredTimesheet, notifiedUserList);
            }

            var paragraph = new TextParagraph { Text = text };
            var card = ChatEventFactory.CreateCard(paragraph);

            if (address != null)
            {
                await connector.SendMessageAsync(null, address, card);
            }
            else if (email != null)
            {
                await _mailService.SendMailAsync("Users not notified", text, email);
            }
        }

        private async Task<IReadOnlyList<string>> NotifyUsersOverChatAsync(
            IHangoutsChatConnector connector,
            TimesheetStates state,
            Timesheet[] timesheets)
        {
            var notifiedUserList = new List<string>();
            var addressesForUpdate = new List<GoogleAddress>();
            var storeAddresses = await _storageService.GetAddressesAsync();
            var emails = timesheets.Select(it => it.UserEmail).ToArray();
            var filteredAddresses = storeAddresses.Where(it => emails.Contains(it.UserEmail)).ToArray();

            IReadOnlyList<GoogleAddress> privateAddresses = new GoogleAddress[0];
            if (filteredAddresses.Length < timesheets.Length)
            {
                var storeAddressesNames = storeAddresses.Select(it => it.SpaceName).Distinct().ToArray();
                privateAddresses = connector
                    .GetPrivateAddress(storeAddressesNames)
                    .Select(it => new GoogleAddress
                    {
                        SpaceName = it.Space.Name,
                        UserName = it.Sender.Name,
                        UserDisplayName = it.Sender.DisplayName
                    })
                    .ToArray();

                // Store inactive spaces, so not to be requested the next time
                addressesForUpdate.AddRange(privateAddresses.Where(it => it.UserName == null));
            }

            var textMessage = OpenAirText.GetText(state, OpenAirTextTypes.Notify);
            foreach (var timesheet in timesheets)
            {
                var message = timesheet.UserName + textMessage;
                var addr = filteredAddresses.FirstOrDefault(it => it.UserEmail == timesheet.UserEmail);
                if (addr == null)
                {
                    addr = privateAddresses.FirstOrDefault(it => it.UserDisplayName == timesheet.UserName);
                    if (addr == null)
                    {
                        continue;
                    }

                    addr.UserEmail = timesheet.UserEmail;

                    addressesForUpdate.Add(addr);
                }

                notifiedUserList.Add(timesheet.UserName);

                await connector.SendMessageAsync(
                    message,
                    new GoogleChatAddress(addr.SpaceName, string.Empty, "DM", addr.UserName, addr.UserDisplayName));
            }

            if (addressesForUpdate.Count > 0)
            {
                await _storageService.AddAddressesAsync(addressesForUpdate);
            }

            return notifiedUserList;
        }
    }
}
