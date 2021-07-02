using System.IO;
using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MentorBot.Tests._Base
{
    internal class MockHttpResponseData : HttpResponseData
    {
        /// <summary>Initializes a new instance of the <see cref="MockHttpResponseData"/> class.</summary>
        public MockHttpResponseData(FunctionContext functionContext)
            : base (functionContext)
        {
        }

        /// <inheritdoc/>
        public override HttpStatusCode StatusCode { get; set; }

        /// <inheritdoc/>
        public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();

        /// <inheritdoc/>
        public override Stream Body { get; set; } = new MemoryStream();

        /// <inheritdoc/>
        public override HttpCookies Cookies => null;
    }
}
