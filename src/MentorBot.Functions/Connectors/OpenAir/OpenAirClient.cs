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
    public sealed partial class OpenAirClient
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

        /// <summary>Creates the request body.</summary>
        public static Request CreateRequest(OpenAirOptions options, string username, string password) =>
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

        /// <summary>Executes the request asynchronous.</summary>
        /// <typeparam name="T">The type of the request result.</typeparam>
        public static async Task<T> ExecuteRequestAsync<T>(string uri, Request request, Func<HttpMessageHandler> httpMessageHandlerFactory)
        {
            using (var messageHandler = httpMessageHandlerFactory())
            {
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
        }

        /// <summary>Gets the timesheets asynchronous.</summary>
        public async Task<Timesheet[]> GetTimesheetsAsync(DateTime startDate, DateTime endDate)
        {
            var req = CreateRequest(_options, _options.OpenAirUserName, _options.OpenAirPassword);

            req.Read = new Read
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
            };

            var result = await ExecuteRequestAsync<Response>(_options.OpenAirUrl, req, _messageHandlerFactory).ConfigureAwait(false);

            return result.Read.Timesheet;
        }

        /// <summary>Gets the user by identifier asynchronous.</summary>
        public async Task<User> GetUserByIdAsync(long userId)
        {
            var req = CreateRequest(_options, _options.OpenAirUserName, _options.OpenAirPassword);

            req.Read = new Read
            {
                Type = DateType.User,
                Method = "equal to",
                Limit = 1,
                User = new[]
                {
                    new User
                    {
                        Id = userId
                    }
                },
                Return = new RaedReturn
                {
                    Content = "<id /><name /><timezone/><addr /><departmentid />"
                }
            };

            var result = await ExecuteRequestAsync<Response>(_options.OpenAirUrl, req, _messageHandlerFactory).ConfigureAwait(false);

            return result.Read.User.FirstOrDefault();
        }

        /// <summary>Gets the user by identifier asynchronous.</summary>
        public async Task<User[]> GetAllUserAsync()
        {
            var req = CreateRequest(_options, _options.OpenAirUserName, _options.OpenAirPassword);

            req.Read = new Read
            {
                Type = DateType.User,
                Method = "all",
                Limit = 1000,
                Return = new RaedReturn
                {
                    Content = "<id /><name /><timezone/><addr /><departmentid /><active />"
                }
            };

            var result = await ExecuteRequestAsync<Response>(_options.OpenAirUrl, req, _messageHandlerFactory).ConfigureAwait(false);

            return result.Read.User;
        }

        /// <summary>Gets all departments asynchronous.</summary>
        public async Task<Department[]> GetAllDepartmentsAsync()
        {
            var req = CreateRequest(_options, _options.OpenAirUserName, _options.OpenAirPassword);

            req.Read = new Read
            {
                Type = DateType.Department,
                Method = "all",
                Limit = 1000,
                Return = new RaedReturn
                {
                    Content = "<id /><name /><userid />"
                }
            };

            var result = await ExecuteRequestAsync<Response>(_options.OpenAirUrl, req, _messageHandlerFactory).ConfigureAwait(false);

            return result.Read.Department;
        }

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
    }
}
