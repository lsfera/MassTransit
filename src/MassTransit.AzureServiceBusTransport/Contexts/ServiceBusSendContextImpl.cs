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
namespace MassTransit.AzureServiceBusTransport.Contexts
{
    using System;
    using System.Threading;
    using Context;


    public class ServiceBusSendContextImpl<T> :
        BaseSendContext<T>,
        ServiceBusSendContext<T>
        where T : class
    {
        public ServiceBusSendContextImpl(T message, CancellationToken cancellationToken)
            : base(message, cancellationToken)
        {
        }

        public DateTime? ScheduledEnqueueTimeUtc { get; set; }

        public string PartitionKey { get; set; }
    }
}