using System.Collections.Generic;
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
        private int _index = 0;

        public List<Response> Responses { get; } = new List<Response>();

        public Response this[int index] => Responses[index];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var res = Responses[_index++];
            if (request.Content != null)
            {
                res.RequestContent = await request.Content.ReadAsByteArrayAsync();
            }

            var response = new HttpResponseMessage(res.ResponseStatusCode)
            {
                Content = new StringContent(res.ResponseContent, Encoding.UTF8)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(res.ResponseContentType);

            return response;
        }

        public MockHttpMessageHandler Set(string content, string contentType = "application/xml", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Responses.Add(new Response
            {
                ResponseContent = content,
                ResponseContentType = contentType,
                ResponseStatusCode = statusCode
            });

            return this;
        }

        public class Response
        {
            public string ResponseContent { get; set; }

            public string ResponseContentType { get; set; }

            public byte[] RequestContent { get; set; }

            public HttpStatusCode ResponseStatusCode { get; set; }
        }
    }
}
