// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Connectors
{
    /// <summary>A connector that provide data from the OpenAir API endpoints.</summary>
    public interface IOpenAirConnector
    {
        /// <summary>Get unsubmitted timesheet to the peaople I have access to.</summary>
        Task<IReadOnlyList<Timesheet>> GetUnsubmittedTimesheetsAsync(DateTime date, TimesheetStates state, string[] filterByCustomers);

        /// <summary>Synchronizes the users from open air to store.</summary>
        Task SyncUsersAsync();
    }
}
