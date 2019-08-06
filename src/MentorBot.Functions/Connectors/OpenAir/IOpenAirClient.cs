using System;
using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.OpenAir.OpenAirClient;

namespace MentorBot.Functions.Connectors.OpenAir
{
    /// <summary>A open air client.</summary>
    public interface IOpenAirClient
    {
        /// <summary>Gets the timesheets asynchronous.</summary>
        Task<Timesheet[]> GetTimesheetsAsync(DateTime startDate, DateTime endDate);

        /// <summary>Gets the timesheets by status asynchronous.</summary>
        Task<Timesheet[]> GetTimesheetsByStatusAsync(DateTime startDate, DateTime endDate, string status);

        /// <summary>Gets all users asynchronous.</summary>
        Task<User[]> GetAllUsersAsync();

        /// <summary>Gets all departments asynchronous.</summary>
        Task<Department[]> GetAllDepartmentsAsync();

        /// <summary>Gets all active customers asynchronous.</summary>
        Task<Customer[]> GetAllActiveCustomersAsync();

        /// <summary>Gets all active bookings asynchronous.</summary>
        Task<Booking[]> GetAllActiveBookingsAsync(DateTime today);
    }
}
