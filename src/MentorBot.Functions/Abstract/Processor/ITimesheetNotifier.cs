using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>Timesheet report notifier.</summary>
    public interface ITimesheetNotifier
    {
        /// <summary>Send timesheet notifications to users as an asynchronous operation.</summary>
        public Task SendTimesheetNotificationsToUsersAsync(
            IReadOnlyList<Timesheet> timesheets,
            string email,
            string[] departments,
            bool notify,
            bool notifyByEmail,
            TimesheetStates state,
            GoogleChatAddress address,
            IHangoutsChatConnector connector);
    }
}
