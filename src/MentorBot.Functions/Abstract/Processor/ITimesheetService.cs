// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>A timesheet service for notifications.</summary>
    public interface ITimesheetService
    {
        /// <summary>Sends the scheduled timesheet notifications asynchronous.</summary>
        Task SendScheduledTimesheetNotificationsAsync(DateTime scheduleDate);
    }
}
