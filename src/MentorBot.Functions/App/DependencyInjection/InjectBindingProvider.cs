// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace MentorBot.Functions.App.DependencyInjection
{
    /// <summary>A binding provider for the inject service attribute.</summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Host.Bindings.IBindingProvider" />
    public class InjectBindingProvider : IBindingProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="InjectBindingProvider"/> class.</summary>
        public InjectBindingProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            IBinding binding = new InjectBinding(_serviceProvider, context?.Parameter.ParameterType);
            return Task.FromResult(binding);
        }
    }
}
