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
namespace MassTransit.ConsumeConnectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;


    public class ConsumerMetadataCache<T> :
        IConsumerMetadataCache<T>
        where T : class
    {
        readonly MessageInterfaceType[] _consumerTypes;
        readonly MessageInterfaceType[] _messageConsumerTypes;

        ConsumerMetadataCache()
        {
            _consumerTypes = GetConsumerMessageTypes().ToArray();
            _messageConsumerTypes = GetMessageConsumerTypes().ToArray();
        }

        public static MessageInterfaceType[] ConsumerTypes => Cached.Metadata.Value.ConsumerTypes;

        public static MessageInterfaceType[] MessageConsumerTypes => Cached.Metadata.Value.MessageConsumerTypes;

        MessageInterfaceType[] IConsumerMetadataCache<T>.ConsumerTypes => _consumerTypes;

        MessageInterfaceType[] IConsumerMetadataCache<T>.MessageConsumerTypes => _messageConsumerTypes;

        static IEnumerable<MessageInterfaceType> GetConsumerMessageTypes()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IConsumer<>))
            {
                var interfaceType = new MessageInterfaceType(typeof(T).GetGenericArguments()[0], typeof(T));
                if (interfaceType.MessageType.IsValueType == false && interfaceType.MessageType != typeof(string))
                    yield return interfaceType;
            }

            IEnumerable<MessageInterfaceType> types = typeof(T).GetInterfaces()
                .Where(x => x.IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == typeof(IConsumer<>))
                .Select(x => new MessageInterfaceType(x.GetGenericArguments()[0], typeof(T)))
                .Where(x => x.MessageType.IsValueType == false && x.MessageType != typeof(string));

            foreach (MessageInterfaceType type in types)
                yield return type;
        }

        static IEnumerable<MessageInterfaceType> GetMessageConsumerTypes()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IMessageConsumer<>))
            {
                var interfaceType = new MessageInterfaceType(typeof(T).GetGenericArguments()[0], typeof(T));
                if (interfaceType.MessageType.IsValueType == false && interfaceType.MessageType != typeof(string))
                    yield return interfaceType;
            }

            IEnumerable<MessageInterfaceType> types = typeof(T).GetInterfaces()
                .Where(x => x.IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == typeof(IMessageConsumer<>))
                .Select(x => new MessageInterfaceType(x.GetGenericArguments()[0], typeof(T)))
                .Where(x => x.MessageType.IsValueType == false && x.MessageType != typeof(string));

            foreach (MessageInterfaceType type in types)
                yield return type;
        }


        static class Cached
        {
            internal static readonly Lazy<IConsumerMetadataCache<T>> Metadata = new Lazy<IConsumerMetadataCache<T>>(
                () => new ConsumerMetadataCache<T>(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}