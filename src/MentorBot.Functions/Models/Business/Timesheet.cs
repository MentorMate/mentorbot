// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;

namespace MentorBot.Functions.Models.Business
{
    /// <summary>The timesheet business model.</summary>
    public sealed class Timesheet
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the name of the user.</summary>
        public string UserName { get; set; }

        /// <summary>Gets or sets the email of the user.</summary>
        public string UserEmail { get; set; }

        /// <summary>Gets or sets the name of the department.</summary>
        public string DepartmentName { get; set; }

        /// <summary>Gets or sets the name of the manager.</summary>
        public string ManagerName { get; set; }

        /// <summary>Gets or sets the total.</summary>
        public double Total { get; set; }
    }
}
