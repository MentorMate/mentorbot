// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App.DependencyInjection
{
    /// <summary>A binding that inject a service from the service providers.</summary>
    public class InjectBinding : IBinding
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _type;

        /// <summary>Initializes a new instance of the <see cref="InjectBinding"/> class.</summary>
        public InjectBinding(IServiceProvider serviceProvider, Type type)
        {
            _type = type;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public bool FromAttribute => true;

        /// <inheritdoc/>
        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context) =>
            Task.FromResult((IValueProvider)new InjectValueProvider(value));

        /// <inheritdoc/>
        public async Task<IValueProvider> BindAsync(BindingContext context)
        {
            await Task.Yield();
            var value = _serviceProvider.GetRequiredService(_type);
            return await BindAsync(value, context?.ValueContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();

        private class InjectValueProvider : IValueProvider
        {
            private readonly object _value;

            public InjectValueProvider(object value)
            {
                _value = value;
            }

            public Type Type => _value.GetType();

            public Task<object> GetValueAsync() => Task.FromResult(_value);

            public string ToInvokeString() => _value.ToString();
        }
    }
}
