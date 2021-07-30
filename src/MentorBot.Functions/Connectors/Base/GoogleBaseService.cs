using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.Base
{
    /// <summary>The base class for all google connectors. This class provide helper methods like create new google service instance.</summary>
    /// <typeparam name="T">The type of the google service.</typeparam>
    public class GoogleBaseService<T>
        where T : BaseClientService
    {
        /// <summary>Initializes a new instance of the <see cref="GoogleBaseService{T}"/> class.</summary>
        protected GoogleBaseService(Lazy<T> serviceProviderFactory)
        {
            ServiceProviderFactory = serviceProviderFactory;
        }

        /// <summary>Gets the service provider factory.</summary>
        protected Lazy<T> ServiceProviderFactory { get; }

        /// <summary>Gets the service provide.</summary>
        protected T ServiceProvider => ServiceProviderFactory.Value;

        /// <summary>Creates the base client service initializer by service account.</summary>
        [ExcludeFromCodeCoverage]
        public static BaseClientService.Initializer InitByServiceAccount(string name, Stream serviceAccountStream, params string[] scopes) =>
            new BaseClientService.Initializer
            {
                ApplicationName = name,
                HttpClientInitializer = GoogleCredential.FromStream(serviceAccountStream).CreateScoped(scopes)
            };

        /// <summary>Creates the base client service initializer by api key.</summary>
        public static BaseClientService.Initializer InitByKey(GoogleCloudOptions options) =>
            new BaseClientService.Initializer
            {
                ApplicationName = options.GoogleCloudApplicationName,
                ApiKey = options.GoogleCloudApiKey
            };
    }
}
