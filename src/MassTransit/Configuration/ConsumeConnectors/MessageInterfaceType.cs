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


    public class MessageInterfaceType
    {
        readonly Lazy<IMessageConnectorFactory> _consumeConnectorFactory;
        readonly Lazy<IMessageConnectorFactory> _consumeMessageConnectorFactory;

        public MessageInterfaceType(Type messageType, Type consumerType)
        {
            MessageType = messageType;

            _consumeConnectorFactory = new Lazy<IMessageConnectorFactory>(() => (IMessageConnectorFactory)
                Activator.CreateInstance(typeof(ConsumeMessageConnectorFactory<,>).MakeGenericType(consumerType,
                    messageType)));

            _consumeMessageConnectorFactory = new Lazy<IMessageConnectorFactory>(() => (IMessageConnectorFactory)
                Activator.CreateInstance(typeof(LegacyConsumeConnectorFactory<,>).MakeGenericType(
                    consumerType, messageType)));
        }

        public static IEqualityComparer<MessageInterfaceType> MessageTypeComparer { get; } = new MessageTypeEqualityComparer();

        public Type MessageType { get; }

        public IConsumerMessageConnector GetConsumerConnector()
        {
            return _consumeConnectorFactory.Value.CreateConsumerConnector();
        }

        public IConsumerMessageConnector GetConsumerMessageConnector()
        {
            return _consumeMessageConnectorFactory.Value.CreateConsumerConnector();
        }

        public IInstanceMessageConnector GetInstanceConnector()
        {
            return _consumeConnectorFactory.Value.CreateInstanceConnector();
        }

        public IInstanceMessageConnector GetInstanceMessageConnector()
        {
            return _consumeMessageConnectorFactory.Value.CreateInstanceConnector();
        }


        sealed class MessageTypeEqualityComparer : IEqualityComparer<MessageInterfaceType>
        {
            public bool Equals(MessageInterfaceType x, MessageInterfaceType y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.GetType() != y.GetType())
                    return false;
                return x.MessageType.Equals(y.MessageType);
            }

            public int GetHashCode(MessageInterfaceType obj)
            {
                return obj.MessageType.GetHashCode();
            }
        }
    }
}