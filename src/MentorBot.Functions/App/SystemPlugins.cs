﻿// cSpell:ignore AAAAB, Jhon, Petar
using System.Diagnostics.CodeAnalysis;

using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Processors.BuildInfo;
using MentorBot.Functions.Processors.Issues;
using MentorBot.Functions.Processors.Timesheets;
using MentorBot.Functions.Processors.UserFlow;

namespace MentorBot.Functions.App
{
    /// <summary>A default in-system plugins.</summary>
    public static class SystemPlugins
    {
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000", Justification = "new format")]
        private static readonly Plugin Jira = new()
        {
            Id = "31235913-2cd7-4bb1-9af8-35efa521bb1d",
            Name = "Issues/Ticketing",
            ProcessorTypeName = "MentorBot.Functions.Processors.Issues.IssuesProcessor",
            Enabled = true,
            Groups =
                new[]
                {
                    new PluginPropertyGroup
                    {
                        Name = "Jira Hosts",
                        UniqueName = IssuesProperties.HostsGroup,
                        Multi = false,
                        ObjectType = PropertyObjectTypes.Settings,
                        Properties = new[]
                        {
                            new PluginProperty
                            {
                                Name = "Host",
                                UniqueName = IssuesProperties.Host,
                                ValueType = PropertyValueTypes.String,
                            },
                            new PluginProperty
                            {
                                Name = "Username",
                                UniqueName = IssuesProperties.User,
                                ValueType = PropertyValueTypes.String,
                            },
                            new PluginProperty
                            {
                                Name = "Token",
                                UniqueName = IssuesProperties.Token,
                                ValueType = PropertyValueTypes.String,
                            },
                        },
                    },
                },
            Examples = new[]
            {
                "Get issues for PROJECT in state TO DO",
            },
        };

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000", Justification = "new format")]
        private static readonly Plugin Jenkins =
            new()
            {
                Id = "7239ed4d-5b95-4bdd-be2c-007c281e87e6",
                Name = "Jenkins Build Info",
                ProcessorTypeName = "MentorBot.Functions.Processors.BuildInfo.BuildInfoProcessor",
                Enabled = true,
                Groups =
                    new[]
                    {
                        new PluginPropertyGroup
                        {
                            Name = "Jenkins Hosts",
                            UniqueName = BuildInfoProperties.HostsGroup,
                            Multi = false,
                            ObjectType = PropertyObjectTypes.Settings,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Host",
                                    UniqueName = BuildInfoProperties.Host,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Username",
                                    UniqueName = BuildInfoProperties.User,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Token",
                                    UniqueName = BuildInfoProperties.Token,
                                    ValueType = PropertyValueTypes.String,
                                },
                            },
                        },
                        new PluginPropertyGroup
                        {
                            Name = "Jenkins Jobs",
                            UniqueName = "Jenkins.Jobs",
                            Multi = true,
                            ObjectType = PropertyObjectTypes.User,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Job",
                                    UniqueName = BuildInfoProperties.JobName,
                                    ValueType = PropertyValueTypes.String,
                                },
                            },
                        },
                    },
                Examples = new[]
                {
                    "Show build status",
                },
            };

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000", Justification = "new format")]
        private static readonly Plugin OpenAir =
            new()
            {
                Id = "1e2d563b-88f4-4ab4-896f-89fe5c6a0236",
                Name = "OpenAir",
                ProcessorTypeName = "MentorBot.Functions.Processors.Timesheets.OpenAirProcessor",
                Enabled = true,
                Groups =
                    new[]
                    {
                        new PluginPropertyGroup
                        {
                            Name = "OpenAir Filter Customers",
                            UniqueName = "OpenAir.Filters.Customers",
                            Multi = true,
                            ObjectType = PropertyObjectTypes.Settings,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Customer Name",
                                    UniqueName = TimesheetsProperties.FilterByCustomer,
                                    ValueType = PropertyValueTypes.String,
                                },
                            },
                        },
                        new PluginPropertyGroup
                        {
                            Name = "OpenAir Properties",
                            UniqueName = TimesheetsProperties.UserProperties,
                            Multi = false,
                            ObjectType = PropertyObjectTypes.User,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Max Hours Per Week",
                                    UniqueName = TimesheetsProperties.UserMaxHours,
                                    ValueType = PropertyValueTypes.Number,
                                },
                            },
                        },
                        new PluginPropertyGroup
                        {
                            Name = "Timesheets chat room report",
                            UniqueName = TimesheetsProperties.AutoNotificationsGroup,
                            Multi = true,
                            ObjectType = PropertyObjectTypes.Settings,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Start Hour",
                                    DescriptionHtml =
                                        "Job run every <b>15 min</b>." +
                                        " '16 Fri' 16:00 at friday, '16:30 FRI' is 16:30 at Fri, '18 *' is every day at 18," +
                                        " '1,2 MON,THU,EOM' etc.",
                                    UniqueName = TimesheetsProperties.AutoNotificationsCron,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Report Name",
                                    DescriptionHtml = "unsubmitted, unapproved",
                                    UniqueName = TimesheetsProperties.AutoNotificationsReportName,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Space (comma separated list)",
                                    DescriptionHtml = "room ids taken from google chat url, only id (optional), ex: AAAAB4BP1EQ",
                                    UniqueName = TimesheetsProperties.AutoNotificationsSpaces,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Email",
                                    UniqueName = TimesheetsProperties.AutoNotificationsManagerEmail,
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Departments",
                                    UniqueName = TimesheetsProperties.AutoNotificationsDepartments,
                                    DescriptionHtml = "List of departments to be included or empty for all",
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Notify Users",
                                    DescriptionHtml = "Also, notify unsubmitted users with direct message",
                                    UniqueName = TimesheetsProperties.AutoNotificationsNotify,
                                    ValueType = PropertyValueTypes.Boolean,
                                },
                            },
                        },
                        new PluginPropertyGroup
                        {
                            Name = "Unsubmitted timesheets global statistics report",
                            UniqueName = TimesheetsProperties.GlobalStatisticsGroup,
                            Multi = false,
                            ObjectType = PropertyObjectTypes.Settings,
                            Properties = new[]
                            {
                                new PluginProperty
                                {
                                    Name = "Log Cron",
                                    UniqueName = TimesheetsProperties.GlobalStatisticsCron,
                                    DescriptionHtml = "A cron to log the unsubmitted timesheet global statistics",
                                    ValueType = PropertyValueTypes.String,
                                },
                                new PluginProperty
                                {
                                    Name = "Top manager email",
                                    UniqueName = TimesheetsProperties.GlobalStatisticsEmail,
                                    DescriptionHtml = "The top level manager email to use in OpenAir as top level",
                                    ValueType = PropertyValueTypes.String,
                                },
                            },
                        },
                    },
                Examples = new[]
                {
                    "Get unsubmitted timesheets",
                    "Get unapproved timesheets for the previous week",
                    "Get unsubmitted timesheets in department QA exclude customer 'Test Customer'",
                },
            };

        /// <summary>Gets the system plugins.</summary>
        public static Plugin[] GetSystemPlugins() =>
            new[]
                {
                    Jira,
                    Jenkins,
                    new Plugin
                    {
                        Id = "1a90977d-208a-4d85-a2ba-6366532a4225",
                        Name = "Google Calendar",
                        ProcessorTypeName = "MentorBot.Functions.Processors.CalendarProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Get my next meeting",
                        },
                    },
                    new Plugin
                    {
                        Id = "0930a559-9e53-44d0-8c27-56030a6d4940",
                        Name = "Greetings",
                        ProcessorTypeName = "MentorBot.Functions.Processors.HelloProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Hi",
                        },
                    },
                    new Plugin
                    {
                        Id = "acdf97a9-467a-43fa-b4b2-35dd933870ad",
                        Name = "Help",
                        ProcessorTypeName = "MentorBot.Functions.Processors.HelpProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Help",
                        },
                    },
                    new Plugin
                    {
                        Id = "fdc902a9-7cbc-45b4-9670-5785fa9e9a25",
                        Name = "Time",
                        ProcessorTypeName = "MentorBot.Functions.Processors.LocalTimeProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Get my local time",
                            "Get the time in Varna",
                        },
                    },
                    OpenAir,
                    new Plugin
                    {
                        Id = "720f5079-6677-4e31-a953-a2c6dc7ce637",
                        Name = "Repeater",
                        ProcessorTypeName = "MentorBot.Functions.Processors.RepeatProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "repeat after me This is cool",
                            "repeat delay 10s This is cool",
                        },
                    },
                    new Plugin
                    {
                        Id = "bb1c66bf-6627-4498-9118-1ac6f46ee22d",
                        Name = "Wikipedia",
                        ProcessorTypeName = "MentorBot.Functions.Processors.WikipediaProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Who are The Beatles",
                            "What is Sun",
                            "Who is Petar",
                        },
                    },
                    new Plugin
                    {
                        Id = "52103ec6-5592-4526-af1d-f93132f72d51",
                        Name = "User Info",
                        ProcessorTypeName = "MentorBot.Functions.Processors.UserInfo.UserInfoProcessor",
                        Enabled = true,
                        Examples = new[]
                        {
                            "Get user information for Jhon Doe",
                            "Show user Jhon Doe",
                        },
                    }
                };
    }
}
