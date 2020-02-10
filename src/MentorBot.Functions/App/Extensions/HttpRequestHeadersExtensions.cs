using System;
using System.Net.Http.Headers;
using System.Text;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>A http request headers extensions.</summary>
    public static class HttpRequestHeadersExtensions
    {
        /// <summary>Set a basics authentication.</summary>
        public static void BasicAuthentication(this HttpRequestHeaders headers, string username, string password) =>
            headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Concat(username, ":", password))));
    }
}
