﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Tests
{
    using System;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TestFramework;
    using TestFramework.Messages;


    [TestFixture]
    public class When_a_message_fails_to_deserialize_properly :
        InMemoryTestFixture
    {
        [Test]
        public void It_should_respond_with_a_serialization_fault()
        {
            Assert.Throws<RequestFaultException>(async () => await _response);
        }

        IRequestClient<PingMessage, PongMessage> _requestClient;
        Task<PongMessage> _response;

        [TestFixtureSetUp]
        public void Setup()
        {
            _requestClient = CreateRequestClient<PingMessage, PongMessage>();

            _response = _requestClient.Request(new PingMessage());
        }

        protected override void ConfigureBus(IInMemoryBusFactoryConfigurator configurator)
        {
        }

        protected override void ConfigureInputQueueEndpoint(IReceiveEndpointConfigurator configurator)
        {
            Handler<PingMessage>(configurator, async context =>
            {
                throw new SerializationException("This is fine, forcing death");
            });
        }
    }

    /// <summary>
    /// this requires debugger tricks to make it work
    /// </summary>
    [TestFixture, Explicit]
    public class When_a_message_has_an_unrecognized_body_format :
        InMemoryTestFixture
    {
        Task<ConsumeContext<PingMessage>> _handled;
        Task<ConsumeContext<ReceiveFault>> _faulted;

        [Test]
        public async Task It_should_publish_a_fault()
        {
            await InputQueueSendEndpoint.Send(new PingMessage(), context => context.ContentType = new ContentType("text/json"));

            var faultContext = await _faulted;
        }

        protected override void ConfigureInputQueueEndpoint(IReceiveEndpointConfigurator configurator)
        {
            _handled = Handled<PingMessage>(configurator);

            _faulted = Handled<ReceiveFault>(configurator);
        }
    }
}