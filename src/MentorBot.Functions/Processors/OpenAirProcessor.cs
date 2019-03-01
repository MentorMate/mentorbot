// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public IReadOnlyList<TextDeconstructionInformation> InitalializationCommandDefinitians =>
            new[]
            {
                new TextDeconstructionInformation("Get unsubmited timesheets", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Get unsubmitted timesheets", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Get unsubmited timesheets in department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Get unsubmitted timesheets in department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Get unsubmited timesheets from department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Get unsubmitted timesheets from department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmited timesheets", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmitted timesheets", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmited timesheets in department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmitted timesheets in department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmited timesheets from department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Show unsubmitted timesheets from department", "timesheet", SentenceTypes.Command),
                new TextDeconstructionInformation("Notify unsubmited timesheets", "timesheet", SentenceTypes.Command, "Notify"),
                new TextDeconstructionInformation("Notify unsubmitted timesheets", "timesheet", SentenceTypes.Command, "Notify"),
                new TextDeconstructionInformation("Notify unsubmited timesheets from department", "timesheet", SentenceTypes.Command, "Notify"),
                new TextDeconstructionInformation("Notify unsubmitted timesheets from department", "timesheet", SentenceTypes.Command, "Notify"),
                new TextDeconstructionInformation("Notify unsubmited timesheets in department", "timesheet", SentenceTypes.Command, "Notify"),
                new TextDeconstructionInformation("Notify unsubmitted timesheets in department", "timesheet", SentenceTypes.Command, "Notify")
            };

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            if (info == null)
            {
                return new ValueTask<ChatEventResult>(
                    new ChatEventResult("I do not understand the sentance."));
            }

            var notify = info.TextSentanceChunk.StartsWith("Notify", StringComparison.InvariantCultureIgnoreCase);
            var department = Regex.Match(info.TextSentanceChunk, "department ['\"]?([\\w\\.\\-_]+)['\"]?$");
            var departmentValue = department.Success && department.Groups.Count == 2 ? department.Groups[1].Value : null;

            _openAirConnector.GetUnsubmittedTimesheetsAsync(DateTime.Today)
                .ContinueWith(task => ProcessNotify(
                    task.Result,
                    departmentValue,
                    notify,
                    new GoogleChatAddress(originalChatEvent),
                    responder as IHangoutsChatConnector));

            return new ValueTask<ChatEventResult>(
                new ChatEventResult(text: null));
        }

        private static string GetCardText(IReadOnlyList<Timesheet> timesheets, IReadOnlyList<string> notifiedUserList) =>
            string.Join(string.Empty, timesheets.Where(it => !notifiedUserList.Contains(it.UserName))
                .Select(it => $"<b>{it.UserName}:</b> {it.Total} <i>({it.DepartmentName})</i><br>"));

        /// <summary>Processes the specified timesheets.</summary>
        private async Task ProcessNotify(
            IReadOnlyList<Timesheet> timesheets,
            string department,
            bool notify,
            GoogleChatAddress address,
            IHangoutsChatConnector connector)
        {
            string text;
            var filteredTimesheet =
                department == null ?
                timesheets :
                timesheets
                    .Where(it => it.DepartmentName.Equals(department, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

            var notifiedUserList = new List<string>();
            if (filteredTimesheet.Count == 0)
            {
                text = "<b>All user have submitted timesheets.</b>";
            }
            else if (notify && filteredTimesheet.Count > 0)
            {
                var emails = filteredTimesheet.Select(it => it.UserEmail).ToArray();
                var storeAddresses = await _storageService.GetAddressesAsync();
                var filteredAddresses = storeAddresses.Where(it => emails.Contains(it.UserEmail)).ToArray();

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
                    foreach (var addr in privateAddresses.Where(it => it.UserName == null))
                    {
                        await _storageService.AddAddressAsync(addr);
                    }
                }

                foreach (var timesheet in filteredTimesheet)
                {
                    var message = $"{timesheet.UserName}, Please submit your timesheet. Your current hours are {timesheet.Total}.";
                    var addr = filteredAddresses.FirstOrDefault(it => it.UserEmail == timesheet.UserEmail);
                    if (addr == null)
                    {
                        addr = privateAddresses.FirstOrDefault(it => it.UserDisplayName == timesheet.UserName);
                        if (addr == null)
                        {
                            continue;
                        }

                        addr.UserEmail = timesheet.UserEmail;
                        await _storageService.AddAddressAsync(addr);
                    }

                    notifiedUserList.Add(timesheet.UserName);
                    await connector.SendMessageAsync(
                        message,
                        new GoogleChatAddress(addr.SpaceName, string.Empty, "DM", addr.UserName, addr.UserDisplayName));
                }

                text = notifiedUserList.Count == filteredTimesheet.Count ?
                    "All users width unsubmitted timesheets are notified! Total of " + notifiedUserList.Count :
                    "The following people where not notified: <br>" + GetCardText(filteredTimesheet, notifiedUserList);
            }
            else
            {
                text = "The following people have to submit timesheet: <br>" + GetCardText(filteredTimesheet, notifiedUserList);
            }

            var paragraph = new TextParagraph { Text = text };
            var card = ChatEventFactory.CreateCard(paragraph);

            await connector.SendMessageAsync(null, address, card);
        }
    }
}
