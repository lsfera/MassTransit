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
namespace MassTransit.Context
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Util;


    /// <summary>
    /// Handles the sending of a request to the endpoint specified
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    public class SendRequest<TRequest> :
        IPipe<SendContext<TRequest>>,
        Request<TRequest>
        where TRequest : class
    {
        readonly IBus _bus;
        readonly Action<RequestContext<TRequest>> _callback;
        readonly TaskScheduler _taskScheduler;
        SendRequestContext<TRequest> _requestContext;
        readonly Guid _requestId;

        public SendRequest(IBus bus, TaskScheduler taskScheduler, Action<RequestContext<TRequest>> callback)
        {
            _taskScheduler = taskScheduler;
            _callback = callback;
            _bus = bus;
            _requestId = NewId.NextGuid();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
        }

        Task IPipe<SendContext<TRequest>>.Send(SendContext<TRequest> context)
        {
            context.RequestId = _requestId;
            context.ResponseAddress = _bus.Address;

            if(_requestContext == null)
                _requestContext = new SendRequestContext<TRequest>(_bus, context, _taskScheduler, _callback);
            else
            {
                var publishContext = new PublishRequestContext<TRequest>(_bus, context, _callback, _requestContext.Connections, ((RequestContext<TRequest>)_requestContext).Task);
            }

            return TaskUtil.Completed;
        }

        Task Request<TRequest>.Task => ((RequestContext)_requestContext).Task;
    }
}