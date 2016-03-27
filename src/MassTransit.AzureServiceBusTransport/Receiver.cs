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
namespace MassTransit.AzureServiceBusTransport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contexts;
    using Logging;
    using MassTransit.Pipeline;
    using Microsoft.ServiceBus.Messaging;


    public class Receiver :
        ReceiverMetrics,
        IDisposable
    {
        static readonly ILog _log = Logger.Get<Receiver>();

        readonly TaskCompletionSource<ReceiverMetrics> _completeTask;
        readonly Uri _inputAddress;
        readonly MessageReceiver _messageReceiver;
        readonly IReceiveObserver _receiveObserver;
        readonly IPipe<ReceiveContext> _receivePipe;
        readonly ReceiveSettings _receiveSettings;
        int _currentPendingDeliveryCount;
        long _deliveryCount;
        int _maxPendingDeliveryCount;
        CancellationTokenRegistration _registration;
        bool _shuttingDown;

        public Receiver(MessageReceiver messageReceiver, Uri inputAddress, IPipe<ReceiveContext> receivePipe,
            ReceiveSettings receiveSettings, IReceiveObserver receiveObserver, CancellationToken cancellationToken)
        {
            _messageReceiver = messageReceiver;
            _inputAddress = inputAddress;
            _receivePipe = receivePipe;
            _receiveSettings = receiveSettings;
            _receiveObserver = receiveObserver;

            _completeTask = new TaskCompletionSource<ReceiverMetrics>();

            _registration = cancellationToken.Register(Shutdown);

            var options = new OnMessageOptions
            {
                AutoComplete = false,
                AutoRenewTimeout = receiveSettings.AutoRenewTimeout,
                MaxConcurrentCalls = receiveSettings.MaxConcurrentCalls,
            };

            options.ExceptionReceived += (sender, x) => _completeTask.TrySetException(x.Exception);

            messageReceiver.OnMessageAsync(OnMessage, options);
        }

        public Task<ReceiverMetrics> CompleteTask
        {
            get { return _completeTask.Task; }
        }

        public void Dispose()
        {
            _registration.Dispose();
        }

        public long DeliveryCount
        {
            get { return _deliveryCount; }
        }

        public int ConcurrentDeliveryCount
        {
            get { return _maxPendingDeliveryCount; }
        }

        void Shutdown()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Shutting down receiver: {0}", _inputAddress);

            _shuttingDown = true;

            if (_currentPendingDeliveryCount > 0)
            {
                if (!_completeTask.Task.Wait(TimeSpan.FromSeconds(60)))
                {
                    if (_log.IsWarnEnabled)
                        _log.WarnFormat("Timeout waiting for receiver to exit: {0}", _inputAddress);
                }
            }

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Receiver shutdown completed: {0}", _inputAddress);

            try
            {
                _messageReceiver.Close();
            }
            catch (Exception ex)
            {
                _completeTask.TrySetException(ex);
            }
            finally
            {
                if (_currentPendingDeliveryCount == 0)
                    _completeTask.TrySetResult(this);
            }
        }

        async Task OnMessage(BrokeredMessage message)
        {
            int current = Interlocked.Increment(ref _currentPendingDeliveryCount);
            while (current > _maxPendingDeliveryCount)
                Interlocked.CompareExchange(ref _maxPendingDeliveryCount, current, _maxPendingDeliveryCount);

            long deliveryCount = Interlocked.Increment(ref _deliveryCount);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Receiving {0}:{1} - {2}", deliveryCount, message.MessageId, _receiveSettings.QueueDescription.Path);

            var context = new ServiceBusReceiveContext(_inputAddress, message, _receiveObserver);

            try
            {
                if (_shuttingDown)
                {
                    await _completeTask.Task.ConfigureAwait(false);

                    throw new TransportException(_inputAddress, "Transport shutdown in progress, abandoning message");
                }

                await _receiveObserver.PreReceive(context).ConfigureAwait(false);

                await _receivePipe.Send(context).ConfigureAwait(false);

                await context.CompleteTask.ConfigureAwait(false);

                await message.CompleteAsync().ConfigureAwait(false);

                await _receiveObserver.PostReceive(context).ConfigureAwait(false);

                if (_log.IsDebugEnabled)
                    _log.DebugFormat("Receive completed: {0}", message.MessageId);
            }
            catch (Exception ex)
            {
                if (_log.IsErrorEnabled)
                    _log.Error($"Received faulted: {message.MessageId}", ex);

                await message.AbandonAsync().ConfigureAwait(false);
                await _receiveObserver.ReceiveFault(context, ex).ConfigureAwait(false);
            }
            finally
            {
                int pendingCount = Interlocked.Decrement(ref _currentPendingDeliveryCount);
                if (pendingCount == 0 && _shuttingDown)
                    _completeTask.TrySetResult(this);
            }
        }
    }
}