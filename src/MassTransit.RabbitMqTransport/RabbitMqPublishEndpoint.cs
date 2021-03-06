// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.RabbitMqTransport
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Integration;
    using MassTransit.Pipeline;
    using Topology;
    using Transports;
    using Util;


    public class RabbitMqPublishEndpointProvider :
        IPublishEndpointProvider
    {
        readonly ConcurrentDictionary<Type, Lazy<ISendEndpoint>> _cachedEndpoints;
        readonly IRabbitMqHost _host;
        readonly IMessageNameFormatter _messageNameFormatter;
        readonly PublishObservable _publishObservable;
        readonly IMessageSerializer _serializer;
        readonly Uri _sourceAddress;

        public RabbitMqPublishEndpointProvider(IRabbitMqHost host, IMessageSerializer serializer, Uri sourceAddress)
        {
            _host = host;
            _serializer = serializer;
            _sourceAddress = sourceAddress;
            _messageNameFormatter = host.MessageNameFormatter;
            _cachedEndpoints = new ConcurrentDictionary<Type, Lazy<ISendEndpoint>>();
            _publishObservable = new PublishObservable();
        }

        public IPublishEndpoint CreatePublishEndpoint(Uri sourceAddress, Guid? correlationId, Guid? conversationId)
        {
            return new PublishEndpoint(sourceAddress, this, _publishObservable, correlationId, conversationId);
        }

        public Task<ISendEndpoint> GetPublishSendEndpoint(Type messageType)
        {
            return Task.FromResult(_cachedEndpoints.GetOrAdd(messageType, x => new Lazy<ISendEndpoint>(() => CreateSendEndpoint(x))).Value);
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return _publishObservable.Connect(observer);
        }

        ISendEndpoint CreateSendEndpoint(Type messageType)
        {
            SendSettings sendSettings = _host.GetSendSettings(messageType, _messageNameFormatter);

            ExchangeBindingSettings[] bindings = TypeMetadataCache.GetMessageTypes(messageType)
                .Select(type => type.GetExchangeBinding(_messageNameFormatter))
                .Where(binding => !sendSettings.ExchangeName.Equals(binding.Exchange.ExchangeName))
                .ToArray();

            Uri destinationAddress = _host.Settings.GetSendAddress(sendSettings);

            var modelCache = new RabbitMqModelCache(_host.ConnectionCache);

            var sendTransport = new RabbitMqSendTransport(modelCache, sendSettings, bindings);

            return new SendEndpoint(sendTransport, _serializer, destinationAddress, _sourceAddress);
        }
    }
}