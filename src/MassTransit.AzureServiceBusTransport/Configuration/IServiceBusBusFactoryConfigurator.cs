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
namespace MassTransit.AzureServiceBusTransport.Configuration
{
    using System;


    public interface IServiceBusBusFactoryConfigurator :
        IBusFactoryConfigurator
    {
        /// <summary>
        /// Specify the number of messages to prefetch from the queue to the service
        /// </summary>
        /// <value>The limit</value>
        int PrefetchCount { set; }

        /// <summary>
        /// Specify the number of concurrent consumers (separate from prefetch count)
        /// </summary>
        int MaxConcurrentCalls { set; }

        /// <summary>
        /// Specify a queue name to be used for the bus instance (separate from the receive endpoints queues).
        /// </summary>
        string BusQueueName { set; }

        /// <summary>
        /// Configures a host
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        IServiceBusHost Host(ServiceBusHostSettings settings);

        /// <summary>
        /// Declare a ReceiveEndpoint on the broker and configure the endpoint settings and message consumers.
        /// </summary>
        /// <param name="host">The host for this endpoint</param>
        /// <param name="queueName">The input queue name</param>
        /// <param name="configure">The configuration method</param>
        void ReceiveEndpoint(IServiceBusHost host, string queueName, Action<IServiceBusReceiveEndpointConfigurator> configure);
    }
}