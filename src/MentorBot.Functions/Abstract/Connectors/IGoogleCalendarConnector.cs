// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using Google.Apis.Calendar.v3.Data;

namespace MentorBot.Functions.Abstract.Connectors
{
    /// <summary>A connector that provide data from the google calendar API endpoints.</summary>
    public interface IGoogleCalendarConnector
    {
        /// <summary>Gets the next meeting asynchronous.</summary>
        /// <param name="calendarId">The calendar identifier.</param>
        Task<Event> GetNextMeetingAsync(string calendarId);
    }
}
