﻿using System;
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
using MentorBot.Functions.Processors.LanguageAnalysis;

namespace MentorBot.Functions.Processors.Timesheets
{
    /// <summary>Google chat implementation of <see cref="ITimesheetNotifier" />.</summary>
    public sealed class TimesheetNotifier : ITimesheetNotifier
    {
        private readonly IStorageService _storageService;
        private readonly IMailService _mailService;

        /// <summary>Initializes a new instance of the <see cref="TimesheetNotifier" /> class.</summary>
        public TimesheetNotifier(
            IStorageService storageService,
            IMailService mailService)
        {
            _storageService = storageService;
            _mailService = mailService;
        }

        /// <summary>Processes the specified timesheets.</summary>
        public async Task SendTimesheetNotificationsToUsersAsync(
            IReadOnlyList<Timesheet> timesheets,
            string email,
            string[] departments,
            bool notify,
            bool notifyByEmail,
            TimesheetStates state,
            GoogleChatAddress address,
            IHangoutsChatConnector connector)
        {
            string text;
            var filteredTimesheet = timesheets
                .Where(it =>
                    departments == null ||
                    !departments.Any() ||
                    departments.Any(dep => dep.Equals(it.DepartmentName, StringComparison.InvariantCultureIgnoreCase)))
                .OrderBy(it => it.ManagerName)
                    .ThenBy(it => it.UserName)
                .ToArray();

            var notifiedUserList = new List<string>();
            if (filteredTimesheet.Length == 0)
            {
                var responses = OpenAirText.GetText(state, OpenAirTextTypes.AllAreDone).Split('|', StringSplitOptions.RemoveEmptyEntries);
                var rand = new Random();
                text = responses[rand.Next(0, responses.Length - 1)];
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
                    string.Format(
                        CultureInfo.InvariantCulture,
                        OpenAirText.GetText(state, OpenAirTextTypes.AllAreNotified),
                        notifiedUserList.Count) :
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
                var emailedUsers = string.Join("</b><br/><b>", notifiedUserList);
                var emailText =
                    $"{text}<br/><br/><b>The following people where notified by a direct massage or email:" +
                    $"<br/><b>{emailedUsers}</b>";
                await _mailService.SendMailAsync("Users not notified", emailText, email);
            }
        }

        private static string GetCardText(IReadOnlyList<Timesheet> timesheets, IReadOnlyList<string> notifiedUserList) =>
            string.Join(string.Empty, timesheets.Where(it => !notifiedUserList.Contains(it.UserName))
                .Select(it =>
                    $"<b>{it.UserName}:</b> {it.Total}/{it.UtilizationInHours} <i>({it.DepartmentName}, {it.ManagerName})</i><br>"));

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

            IReadOnlyList<GoogleAddress> privateAddresses = Array.Empty<GoogleAddress>();
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
