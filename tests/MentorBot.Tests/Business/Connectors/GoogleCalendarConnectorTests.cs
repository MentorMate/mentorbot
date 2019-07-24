using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

using MentorBot.Functions.Connectors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    /// <summary>Tests for <see cref="GoogleCalendarConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class GoogleCalendarConnectorTests
    {
        [TestMethod]
        public async Task GoogleCalendarConnector_ShouldGetNextMeeting()
        {
            var email = "william.wallace@gmail.com";
            var eventResult = new Event() { Start = new EventDateTime { DateTime = DateTime.Now.AddMinutes(1) } };
            var eventsResult = new Events { Items = new List<Event> { eventResult } };

            var clientService = Substitute.For<IClientService>();
            var eventsListRequest = Substitute.ForPartsOf<EventsResource.ListRequest>(clientService, email);
            var eventsResource = Substitute.ForPartsOf<EventsResource>(clientService);
            var service = Substitute.ForPartsOf<CalendarService>();
            var connector = new MockedGoogleCalendarConnector(new Lazy<CalendarService>(service), () => eventsResult);

            service.Events.Returns(eventsResource);
            eventsResource.List(email).Returns(eventsListRequest);

            var result = await connector.GetNextMeetingAsync(email);
            
            Assert.AreEqual(eventsListRequest.CalendarId, email);
            Assert.AreEqual(eventsListRequest.SingleEvents, true);
            Assert.AreEqual(eventsListRequest.ShowDeleted, false);
            Assert.AreEqual(eventsListRequest.MaxResults, 5);
            Assert.AreEqual(result, eventResult);

            // TODO: Can be more accurate
            Assert.IsTrue(eventsListRequest.TimeMin.HasValue);
            Assert.IsTrue(eventsListRequest.TimeMax.HasValue);
        }

        [TestMethod]
        public async Task GoogleCalendarConnector_ShouldReturnNullWhenNotFound()
        {
            var email = "william.wallace@gmail.com";
            var clientService = Substitute.For<IClientService>();
            var eventsListRequest = Substitute.ForPartsOf<EventsResource.ListRequest>(clientService, email);
            var eventsResource = Substitute.ForPartsOf<EventsResource>(clientService);
            var service = Substitute.ForPartsOf<CalendarService>();
            var connector = new MockedGoogleCalendarConnector(
                new Lazy<CalendarService>(service),
                () => throw new GoogleApiException(string.Empty, string.Empty) { HttpStatusCode = System.Net.HttpStatusCode.NotFound });

            service.Events.Returns(eventsResource);
            eventsResource.List(email).Returns(eventsListRequest);

            var result = await connector.GetNextMeetingAsync(email);

            Assert.AreEqual(result, null);
        }

        public class MockedGoogleCalendarConnector : GoogleCalendarConnector
        {
            private Func<object> _action;

            public MockedGoogleCalendarConnector(Lazy<CalendarService> service, Func<object> action)
                : base(service)
            {
                _action = action;
            }

            protected override Task<T> ExecuteAsync<T>(CalendarBaseServiceRequest<T> request) =>
                Task.FromResult((T)_action());
        }
    }
}
