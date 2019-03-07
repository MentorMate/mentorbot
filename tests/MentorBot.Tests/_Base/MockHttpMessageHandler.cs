using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MentorBot.Tests.Base
{
    /// <summary>A mocked http message handler.</summary>
    public sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        public byte[] Content { get; set; }

        public string ResponseContent { get; set; } = string.Empty;

        public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;

        public string ContentType { get; set; } = "application/xml";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                Content = await request.Content.ReadAsByteArrayAsync();
            }

            var response = new HttpResponseMessage(ResponseStatusCode)
            {
                Content = new StringContent(ResponseContent, Encoding.UTF8)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            return response;
        }
    }
}
