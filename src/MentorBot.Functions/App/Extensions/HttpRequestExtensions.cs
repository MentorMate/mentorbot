using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Azure.Core.Serialization;

using Microsoft.Azure.Functions.Worker.Http;

#nullable enable

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>A http request headers extensions.</summary>
    public static class HttpRequestExtensions
    {
        private static readonly JsonObjectSerializer AzureJsonSerializer =
            new (SerializerOptions);

        /// <summary>Gets azure json serializer options.</summary>
        public static JsonSerializerOptions SerializerOptions =>
             new JsonSerializerOptions(JsonSerializerDefaults.Web)
             {
                 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                 AllowTrailingCommas = true,
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
             };

        /// <summary>Set a basics authentication.</summary>
        public static void BasicAuthentication(this HttpRequestHeaders headers, string username, string password) =>
            headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Concat(username, ":", password))));

        /// <summary>Gets the first header value.</summary>
        public static string? GetValue(this HttpHeaders headers, string key) =>
            headers.GetValues(key).FirstOrDefault();

        /// <summary>Reads the content asynchronous.</summary>
        /// <typeparam name="T">The type of the context model.</typeparam>
        public static ValueTask<T?> ReadAsAsync<T>(this HttpRequestData req) =>
            req.Body.CanRead ?
            req.ReadFromJsonAsync<T>(AzureJsonSerializer) :
            new ValueTask<T?>(default(T));

        /// <summary>Reads the content asynchronous.</summary>
        /// <typeparam name="T">The type of the context model.</typeparam>
        public static async ValueTask<T?> ReadAsAsync<T>(this HttpContent context) =>
            (T?)await AzureJsonSerializer.DeserializeAsync(
                await context.ReadAsStreamAsync(),
                typeof(T),
                System.Threading.CancellationToken.None);

        /// <summary>Creates the content response.</summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        public static async ValueTask<HttpResponseData> CreateContentResponseAsync<TModel>(this HttpRequestData req, TModel instance)
        {
            var res = req.CreateResponse();
            await res.WriteAsJsonAsync(instance, AzureJsonSerializer);
            return res;
        }
    }
}
