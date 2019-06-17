// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
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
        public Task<Timesheet[]> GetTimesheetsAsync(DateTime startDate, DateTime endDate) =>
            ReadAsync(
                new Read
                {
                    Type = DateType.Timesheet,
                    Filter = "newer-than,older-than",
                    Field = "starts,starts",
                    Date = new[]
                    {
                        Date.Create(startDate),
                        Date.Create(endDate)
                    },
                    Return = new RaedReturn
                    {
                        Content = "<status/><name /><total/><notes /><userid /><starts />"
                    }
                },
                result => result.Timesheet ?? new Timesheet[0]);

        /// <summary>Gets all users asynchronous.</summary>
        public Task<User[]> GetAllUsersAsync() =>
            Task.WhenAll(GetUsersByActiveAsync(true), GetUsersByActiveAsync(false))
                .ContinueWith(task => task.Result.SelectMany(it => it).ToArray());

        /// <summary>Gets all users by active flag asynchronous.</summary>
        public Task<User[]> GetUsersByActiveAsync(bool active) =>
            ReadAsync(
                new Read
                {
                    Type = DateType.User,
                    Method = "equal to",
                    Limit = 1000,
                    User = new[]
                    {
                        new User
                        {
                            Active = active
                        }
                    },
                    Return = new RaedReturn
                    {
                        Content = "<id /><name /><addr /><departmentid /><active /><line_managerid /><user_locationid />"
                    }
                },
                result => result.User);

        /// <summary>Gets all departments asynchronous.</summary>
        public Task<Department[]> GetAllDepartmentsAsync() =>
            ReadAsync(
                new Read
                {
                    Type = DateType.Department,
                    Method = "all",
                    Limit = 1000,
                    Return = new RaedReturn
                    {
                        Content = "<id /><name /><userid />"
                    }
                },
                result => result.Department);

        /// <summary>Gets all active customers asynchronous.</summary>
        /// TODO: Try to remove the static dates.
        public Task<Customer[]> GetAllActiveCustomersAsync() =>
            Task.WhenAll(
                    GetAllActiveCustomersByCreaetedDateAsync(null, new DateTime(2016, 10, 1)),
                    GetAllActiveCustomersByCreaetedDateAsync(new DateTime(2016, 10, 1), null))
                .ContinueWith(task => task.Result.SelectMany(it => it).ToArray());

        /// <summary>Gets all active customers asynchronous.</summary>
        public Task<Customer[]> GetAllActiveCustomersByCreaetedDateAsync(DateTime? from, DateTime? to) =>
            ReadAsync(
                new Read
                {
                    Type = DateType.Customer,
                    Method = "equal to",
                    Limit = 1000,
                    Filter = NotNullOf(from.HasValue ? "newer-than" : null, to.HasValue ? "older-than" : null).First(),
                    Field = "createtime",
                    Date = NotNullOf(
                        from.HasValue ? Date.Create(from.Value) : null,
                        to.HasValue ? Date.Create(to.Value) : null),
                    Customer = new[]
                    {
                        new Customer
                        {
                            Active = true
                        }
                    },
                    Return = new RaedReturn
                    {
                        Content = "<id/><name />"
                    }
                },
                result => result.Customer);

        /// <summary>Gets all active bookings asynchronous.</summary>
        public Task<Booking[]> GetAllActiveBookingsAsync(DateTime today) =>
            ReadAsync(
                new Read
                {
                    Type = DateType.Booking,
                    Limit = 1000,
                    Method = "equal to",
                    Filter = "newer-than,older-than",
                    Field = "enddate,startdate",
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
                    Return = new RaedReturn
                    {
                        Content = "<id/><userid/><ownerid /><projectid /><customerid /><booking_typeid />"
                    }
                },
                result => result.Booking);

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

        private static T[] NotNullOf<T>(params T[] values)
            where T : class =>
            values.Where(it => it != null).ToArray();

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
