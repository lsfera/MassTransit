﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit
{
    using System;
    using ConsumeConfigurators;
    using ConsumeConnectors;


    public static class ObserverExtensions
    {
        /// <summary>
        /// Subscribes an object instance to the bus
        /// </summary>
        /// <param name="configurator">Service Bus Service Configurator 
        /// - the item that is passed as a parameter to
        /// the action that is calling the configurator.</param>
        /// <param name="observer">The observer to connect to the endpoint</param>
        /// <returns>An instance subscription configurator.</returns>
        public static IObserverConfigurator<T> Observer<T>(this IReceiveEndpointConfigurator configurator, IObserver<ConsumeContext<T>> observer)
            where T : class
        {
            var observerConfigurator = new ObserverConfigurator<T>(observer);

            configurator.AddEndpointSpecification(observerConfigurator);

            return observerConfigurator;
        }

        /// <summary>
        /// Subscribes an object instance to the bus
        /// </summary>
        /// <param name="configurator">Service Bus Service Configurator 
        /// - the item that is passed as a parameter to
        /// the action that is calling the configurator.</param>
        /// <param name="observer">The observer to connect to the endpoint</param>
        /// <param name="configureCallback"></param>
        /// <returns>An instance subscription configurator.</returns>
        public static IObserverConfigurator<T> Observer<T>(this IReceiveEndpointConfigurator configurator, IObserver<ConsumeContext<T>> observer,
            Action<IObserverConfigurator<T>> configureCallback)
            where T : class
        {
            var observerConfigurator = new ObserverConfigurator<T>(observer);

            configureCallback(observerConfigurator);

            configurator.AddEndpointSpecification(observerConfigurator);

            return observerConfigurator;
        }

        /// <summary>
        /// Adds a message observer to the service bus for handling a specific type of message
        /// </summary>
        /// <typeparam name="T">The message type to handle, often inferred from the callback specified</typeparam>
        /// <param name="bus"></param>
        /// <param name="observer">The callback to invoke when messages of the specified type arrive on the service bus</param>
        public static ConnectHandle ConnectObserver<T>(this IBus bus, IObserver<ConsumeContext<T>> observer)
            where T : class
        {
            return ObserverConnectorCache<T>.Connector.Connect(bus, observer);
        }

        /// <summary>
        /// Subscribe a request observer to the bus's endpoint
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="requestId"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static ConnectHandle ConnectRequestObserver<T>(this IBus bus, Guid requestId, IObserver<ConsumeContext<T>> observer)
            where T : class
        {
            return ObserverConnectorCache<T>.Connector.Connect(bus, requestId, observer);
        }
    }
}