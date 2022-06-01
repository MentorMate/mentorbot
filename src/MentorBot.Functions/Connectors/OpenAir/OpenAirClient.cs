// cSpell:ignore ownerid, projectid, customerid, departmentid, locationid
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.OpenAir
{
    /// <summary>An OpenAir client.</summary>
    public sealed partial class OpenAirClient : IOpenAirClient
    {
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;
        private readonly OpenAirOptions _options;

        /// <summary>Initializes a new instance of the <see cref="OpenAirClient"/> class.</summary>
        public OpenAirClient(OpenAirOptions options)
            : this(() => new HttpClientHandler(), options)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="OpenAirClient"/> class.</summary>
        public OpenAirClient(Func<HttpMessageHandler> messageHandlerFactory, OpenAirOptions options)
        {
            _messageHandlerFactory = messageHandlerFactory;
            _options = options;
        }

        /// <summary>Gets the timesheets asynchronous.</summary>
        public Task<IReadOnlyList<Timesheet>> GetTimesheetsAsync(DateTime startDate, DateTime endDate) =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.Timesheet,
                        Filter = "newer-than,older-than",
                        Field = "starts,starts",
                        Limit = $"{skip},{take}",
                        Date = new[]
                        {
                            Date.Create(startDate),
                            Date.Create(endDate)
                        },
                        Return = new ReadReturn
                        {
                            Content = "<status/><name /><total/><notes /><userid /><starts />"
                        }
                    },
                    result => result.Timesheet ?? new Timesheet[0]));

        /// <summary>Gets the timesheets by status asynchronous.</summary>
        public Task<IReadOnlyList<Timesheet>> GetTimesheetsByStatusAsync(DateTime startDate, DateTime endDate, string status) =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.Timesheet,
                        Filter = "newer-than,older-than",
                        Field = "starts,starts",
                        Method = "equal to",
                        Limit = $"{skip},{take}",
                        Date = new[]
                        {
                            Date.Create(startDate),
                            Date.Create(endDate),
                        },
                        Timesheet = new[]
                        {
                            new Timesheet { Status = status },
                        },
                        Return = new ReadReturn
                        {
                            Content = "<status/><name /><total/><notes /><userid /><starts />",
                        },
                    },
                    result => result.Timesheet ?? new Timesheet[0]));

        /// <summary>Gets all users asynchronous.</summary>
        public Task<IReadOnlyList<User>> GetAllUsersAsync() =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.User,
                        Limit = $"{skip},{take}",
                        EnableCustom = true,
                        Return = new ReadReturn
                        {
                            Content =
                                "<id /><name /><addr /><departmentid /><active /><line_managerid /><user_locationid /><usr_start_date__c/>"
                        }
                    },
                    result => result.User));

        /// <summary>Gets all departments asynchronous.</summary>
        public Task<IReadOnlyList<Department>> GetAllDepartmentsAsync() =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.Department,
                        Limit = $"{skip},{take}",
                        Return = new ReadReturn
                        {
                            Content = "<id /><name /><userid />"
                        }
                    },
                    result => result.Department));

        /// <summary>Gets all active customers asynchronous.</summary>
        public Task<IReadOnlyList<Customer>> GetAllActiveCustomersAsync() =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.Customer,
                        Method = "equal to",
                        Limit = $"{skip},{take}",
                        Customer = new[]
                        {
                            new Customer
                            {
                                Active = true
                            }
                        },
                        Return = new ReadReturn
                        {
                            Content = "<id/><name />"
                        }
                    },
                    result => result.Customer));

        /// <summary>Gets all active bookings asynchronous.</summary>
        public Task<IReadOnlyList<Booking>> GetAllActiveBookingsAsync(DateTime today) =>
            GetInBatchesAsync((skip, take) =>
                ReadAsync(
                    new Read
                    {
                        Type = DateType.Booking,
                        Method = "equal to",
                        Filter = "newer-than,older-than",
                        Field = "enddate,startdate",
                        Limit = $"{skip},{take}",
                        Date = new[]
                        {
                            Date.Create(today.AddDays(-1)),
                            Date.Create(today.AddDays(1))
                        },
                        Booking = new[]
                        {
                            new Booking
                            {
                                ApprovalStatus = "A"
                            }
                        },
                        Return = new ReadReturn
                        {
                            Content = "<id/><userid/><ownerid /><projectid /><customerid /><booking_typeid />"
                        }
                    },
                    result => result.Booking));

        private static async Task<IReadOnlyList<T>> GetInBatchesAsync<T>(Func<int, int, Task<T[]>> func)
        {
            const int take = 1000;
            var collection = new List<T>();
            var skip = 0;
            int count;

            do
            {
                var executeTask = func(skip, take);
                var batch = await executeTask;
                collection.AddRange(batch);

                count = batch.Length;
                skip += take;
            }
            while (count == take);

            return collection;
        }

        /// <summary>Creates the request body.</summary>
        private static Request CreateRequest(OpenAirOptions options, string username, string password) =>
            new Request
            {
                Client = options.OpenAirCompany,
                Key = options.OpenAirApiKey,
                Auth = new Auth
                {
                    Login = new Login
                    {
                        Company = options.OpenAirCompany,
                        User = username,
                        Password = password
                    }
                }
            };

        private static string Serialize<T>(T model)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(typeof(T));
            var ns = new XmlSerializerNamespaces();
            var ws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            ns.Add(string.Empty, string.Empty);

            using (var xw = XmlWriter.Create(sb, ws))
            {
                serializer.Serialize(xw, model, ns);

                xw.Flush();

                return sb.ToString();
            }
        }

        /// <summary>Executes the request asynchronous.</summary>
        /// <typeparam name="T">The type of the request result.</typeparam>
        private static async Task<T> ExecuteRequestAsync<T>(string uri, Request request, Func<HttpMessageHandler> httpMessageHandlerFactory)
        {
            using (var messageHandler = httpMessageHandlerFactory())
            using (var client = new HttpClient(messageHandler, false))
            {
                HttpResponseMessage response;
                using (var stringContent = new StringContent(Serialize(request)))
                {
                    response = await client.PostAsync(uri, stringContent);
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStreamAsync();
                var deserializer = new XmlSerializer(typeof(T));

                return (T)deserializer.Deserialize(content);
            }
        }

        private async Task<T> ReadAsync<T>(Read read, Func<Read, T> func)
        {
            var req = CreateRequest(_options, _options.OpenAirUserName, _options.OpenAirPassword);

            req.Read = read;

            var result = await ExecuteRequestAsync<Response>(_options.OpenAirUrl, req, _messageHandlerFactory).ConfigureAwait(false);

            return func(result.Read);
        }
    }
}
