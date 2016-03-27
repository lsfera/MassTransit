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


    public static class MessageHeaders
    {
        /// <summary>
        /// The reason for a message action being taken 
        /// </summary>
        public const string Reason = "MT-Reason";

        /// <summary>
        /// The exception message from a Fault
        /// </summary>
        public const string FaultMessage = "MT-Fault-Message";

        /// <summary>
        /// The stack trace from a Fault
        /// </summary>
        public const string FaultStackTrace = "MT-Fault-StackTrace";

        /// <summary>
        /// The endpoint that forwarded the message to the new destination
        /// </summary>
        public const string ForwarderAddress = "MT-Forwarder-Address";

        /// <summary>
        /// The address where the message was originally delivered before being rescheduled
        /// </summary>
        public const string DeliveredAddress = "MT-Scheduling-DeliveredAddress";

        /// <summary>
        /// The tokenId for the message that was registered with the scheduler
        /// </summary>
        public const string SchedulingTokenId = "MT-Scheduling-TokenId";

        /// <summary>
        /// The number of times the message has been redelivered (zero if never)
        /// </summary>
        public const string RedeliveryCount = "MT-Redelivery-Count";

        /// <summary>
        /// The trigger key that was used when the scheduled message was trigger
        /// </summary>
        public const string QuartzTriggerKey = "MT-Quartz-TriggerKey";

        public static Guid? GetSchedulingTokenId(this ConsumeContext context)
        {
            return context.Headers.Get(SchedulingTokenId, default(Guid?));
        }
    }
}