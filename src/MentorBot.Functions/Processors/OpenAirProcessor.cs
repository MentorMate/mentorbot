// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Services
{
    /// <summary>The processor that handle timesheet operations.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class OpenAirProcessor : ICommandProcessor
    {
        private readonly IOpenAirConnector _openAirConnector;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="OpenAirProcessor"/> class.</summary>
        public OpenAirProcessor(
            IOpenAirConnector openAirConnector,
            IStorageService storageService)
        {
            _openAirConnector = openAirConnector;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public string Subject => "Timesheets";

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            var notify = info.TextSentanceChunk.StartsWith("Notify", StringComparison.InvariantCultureIgnoreCase);
            var departmentValue = info.Entities.GetValueOrDefault(nameof(Department))?.FirstOrDefault()?.Replace(". ", ".", StringComparison.InvariantCulture);
            var customersValue = info.Entities.GetValueOrDefault(nameof(Customer));
            var period = info.Entities.GetValueOrDefault("Period")?.FirstOrDefault();
            var state = GetState(info.Entities.GetValueOrDefault("State")?.FirstOrDefault());
            var today = DateTime.Today;
            var date = period == "last week" || period == "previous week" || period == "for the last week" ? today.AddDays(-((int)today.DayOfWeek + 1)) : today;
            if (state == TimesheetStates.None)
            {
                return new ValueTask<ChatEventResult>(
                    new ChatEventResult("Provide a state of the time sheets, like unsubmitted or unapproved!"));
            }

            _openAirConnector.GetUnsubmittedTimesheetsAsync(date, state, customersValue)
                .ContinueWith(task => ProcessNotifyAsync(
                    task.Result,
                    departmentValue,
                    notify,
                    state,
                    new GoogleChatAddress(originalChatEvent),
                    responder as IHangoutsChatConnector));

            return new ValueTask<ChatEventResult>(
                new ChatEventResult(text: null));
        }

        private static string GetCardText(IReadOnlyList<Timesheet> timesheets, IReadOnlyList<string> notifiedUserList) =>
            string.Join(string.Empty, timesheets.Where(it => !notifiedUserList.Contains(it.UserName))
                .Select(it => $"<b>{it.UserName}:</b> {it.Total} <i>({it.DepartmentName})</i><br>"));

        private static TimesheetStates GetState(string state)
        {
            switch (state)
            {
                case "unsibmitted":
                case "unsubmitted":
                case "unsubmited":
                case "not unsibmited":
                case "not unsibmitted":
                case "not unsubmited":
                case "not unsubmitted":
                case "didn ' t submit":
                    return TimesheetStates.Unsubmitted;
                case "unapproved":
                case "unaproved":
                case "not approved":
                case "not aproved":
                case "non approved":
                    return TimesheetStates.Unapproved;
                default:
                    return TimesheetStates.None;
            }
        }

        /// <summary>Processes the specified timesheets.</summary>
        private async Task ProcessNotifyAsync(
            IReadOnlyList<Timesheet> timesheets,
            string department,
            bool notify,
            TimesheetStates state,
            GoogleChatAddress address,
            IHangoutsChatConnector connector)
        {
            string text;
            var myemail = address.Sender.Email;
            var stateText = state == TimesheetStates.Unapproved ? "approved" : "submitted";
            var stateText2 = state == TimesheetStates.Unapproved ? "approve" : "submite";
            var filteredTimesheet = timesheets
                .Where(it =>
                    myemail.Equals(it.DepartmentOwnerEmail, StringComparison.InvariantCultureIgnoreCase) ||
                    myemail.Equals(it.UserManagerEmail, StringComparison.InvariantCultureIgnoreCase))
                .Where(it =>
                    department == null ||
                    department.Equals(it.DepartmentName, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var notifiedUserList = new List<string>();
            if (filteredTimesheet.Length == 0)
            {
                text = $"<b>All user have {stateText} timesheets.</b>";
            }
            else if (notify && filteredTimesheet.Length > 0)
            {
                var emails = filteredTimesheet.Select(it => it.UserEmail).ToArray();
                var storeAddresses = _storageService.GetAddresses();
                var filteredAddresses = storeAddresses.Where(it => emails.Contains(it.UserEmail)).ToArray();
                var addressesForUpdate = new List<GoogleAddress>();

                IReadOnlyList<GoogleAddress> privateAddresses = new GoogleAddress[0];
                if (filteredAddresses.Length < timesheets.Count)
                {
                    var storeAddressesNames = storeAddresses.Select(it => it.SpaceName).ToArray();
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

                foreach (var timesheet in filteredTimesheet)
                {
                    var message = $"{timesheet.UserName}, You have {stateText} timesheet. Your current hours are {timesheet.Total}.";
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

                text = notifiedUserList.Count == filteredTimesheet.Length ?
                    $"All users with not {stateText} timesheets are notified! Total of {notifiedUserList.Count}." :
                    "The following people where not notified: <br>" + GetCardText(filteredTimesheet, notifiedUserList);
            }
            else
            {
                text = $"The following people have to {stateText2} timesheet: <br>" + GetCardText(filteredTimesheet, notifiedUserList);
            }

            var paragraph = new TextParagraph { Text = text };
            var card = ChatEventFactory.CreateCard(paragraph);

            await connector.SendMessageAsync(null, address, card);
        }
    }
}
