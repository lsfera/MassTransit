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
namespace MassTransit.Transports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Pipeline;


    /// <summary>
    /// A receive endpoint is called by the receive transport to push messages to consumers.
    /// The receive endpoint is where the initial deserialization occurs, as well as any additional
    /// filters on the receive context. 
    /// </summary>
    public class ReceiveEndpoint :
        IReceiveEndpoint
    {
        readonly IReceivePipe _receivePipe;
        readonly IReceiveTransport _receiveTransport;

        public ReceiveEndpoint(IReceiveTransport receiveTransport, IReceivePipe receivePipe)
        {
            _receiveTransport = receiveTransport;
            _receivePipe = receivePipe;
        }

        ReceiveEndpointHandle IReceiveEndpoint.Start()
        {
            ReceiveTransportHandle transportHandle = _receiveTransport.Start(_receivePipe);

            return new Handle(transportHandle);
        }

        public ConnectHandle ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _receiveTransport.ConnectReceiveObserver(observer);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            ProbeContext scope = context.CreateScope("receiveEndpoint");

            _receiveTransport.Probe(scope);

            _receivePipe.Probe(scope);
        }

        ConnectHandle IReceiveEndpointObserverConnector.ConnectReceiveEndpointObserver(IReceiveEndpointObserver observer)
        {
            return _receiveTransport.ConnectReceiveEndpointObserver(observer);
        }

        public ConnectHandle ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _receivePipe.ConnectConsumeObserver(observer);
        }

        public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class
        {
            return _receivePipe.ConnectConsumeMessageObserver(observer);
        }


        class Handle :
            ReceiveEndpointHandle
        {
            readonly ReceiveTransportHandle _transportHandle;

            public Handle(ReceiveTransportHandle transportHandle)
            {
                _transportHandle = transportHandle;
            }

            void IDisposable.Dispose()
            {
                _transportHandle.Dispose();
            }

            Task ReceiveEndpointHandle.Stop(CancellationToken cancellationToken)
            {
                return _transportHandle.Stop(cancellationToken);
            }
        }
    }
}