using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveDomain.Messaging.Bus;
using ReactiveDomain.Messaging.Testing;
using Xunit;

namespace ReactiveDomain.Messaging.Tests
{
    public class TestConnectedBusFixture :
                    IDisposable,
                    IHandle<AckCommand>,
                    IHandle<CommandResponse>,
                    IHandleCommand<TestCommands.Fail>,
                    IHandleCommand<TestCommands.RemoteHandled>,
                    IHandleCommand<TestCommands.LocallyHandled>,
                    IHandleCommand<TestCommands.ChainedCaller>,
                    IHandleCommand<TestCommands.TempSubscribed>,
                    IHandle<TestEvent>
    {
        public readonly IDispatcher LocalDispatcher;
        public readonly IDispatcher RemoteDispatcher;

        public long GotEvent;
        public long GotChainedCaller;
        public long GotRemoteHandled;
        public long GotLocallyHandled;
        public long GotTestFailCommand;
        public long GotTestThrowCommand;
        public long GotTestWrapCommand;
        public long GotAck;
        public long GotCommandResponse;
        public long GotFail;
        public long GotSuccess;
        public long GotCanceled;
        public long ResponseData;
        public long GotTestTempSubscribed;

        public readonly TimeSpan StandardTimeout;

        public TestConnectedBusFixture() 
        {
            StandardTimeout = TimeSpan.FromSeconds(0.2);
            LocalDispatcher = new Dispatcher(nameof(TestConnectedBusFixture), 3, false, StandardTimeout, StandardTimeout);
            RemoteDispatcher = new Dispatcher(nameof(TestConnectedBusFixture), 3, false, StandardTimeout, StandardTimeout);

            var conn = new BusConnector(LocalDispatcher, RemoteDispatcher);

            LocalDispatcher.Subscribe<TestEvent>(this);
            LocalDispatcher.Subscribe<TestCommands.LocallyHandled>(this);
            LocalDispatcher.Subscribe<AckCommand>(this);

            RemoteDispatcher.Subscribe<TestCommands.RemoteHandled>(
                new AdHocCommandHandler<TestCommands.RemoteHandled>(
                    cmd =>
                    {
                        Interlocked.Increment(ref GotRemoteHandled);
                        return true;
                    }));

            Warmup();
        }

        private void Warmup()
        {
            //get all the threads and queues running
            var id = Guid.NewGuid();
            for (int i = 0; i < 50; i++)
            {
                LocalDispatcher.TryFire(new TestCommands.LocallyHandled(id, null));
            }

            for (int i = 0; i < 50; i++)
            {
                RemoteDispatcher.TryFire(new TestCommands.RemoteHandled(id, null));
            }
            SpinWait.SpinUntil(() => LocalDispatcher.Idle);
            SpinWait.SpinUntil(() => RemoteDispatcher.Idle);

            ClearCounters();
        }

        public void ClearCounters()
        {
            Interlocked.Exchange(ref GotChainedCaller, 0);
            Interlocked.Exchange(ref GotRemoteHandled, 0);
            Interlocked.Exchange(ref GotLocallyHandled, 0);
            Interlocked.Exchange(ref GotTestFailCommand, 0);
            Interlocked.Exchange(ref GotTestThrowCommand, 0);
            Interlocked.Exchange(ref GotTestWrapCommand, 0);
            Interlocked.Exchange(ref GotAck, 0);
            Interlocked.Exchange(ref GotCommandResponse, 0);
            Interlocked.Exchange(ref GotFail, 0);
            Interlocked.Exchange(ref GotSuccess, 0);
            Interlocked.Exchange(ref GotCanceled, 0);
            Interlocked.Exchange(ref ResponseData, 0);
            Interlocked.Exchange(ref GotTestTempSubscribed, 0);
        }

        public void Handle(TestEvent evt)
        {
            Interlocked.Increment(ref GotEvent);
        }

        public CommandResponse Handle(TestCommands.ChainedCaller command)
        {
            Interlocked.Increment(ref GotChainedCaller);
            LocalDispatcher.Fire(new TestCommands.LocallyHandled(command.CorrelationId, command.MsgId));
            return command.Succeed();
        }

        public CommandResponse Handle(TestCommands.TempSubscribed command)
        {
            Interlocked.Increment(ref GotTestTempSubscribed);
            return command.Succeed();
        }

        public CommandResponse Handle(TestCommands.RemoteHandled command)
        {
            Interlocked.Increment(ref GotRemoteHandled);
            return command.Succeed();
        }
        public CommandResponse Handle(TestCommands.LocallyHandled command)
        {
            Interlocked.Increment(ref GotLocallyHandled);
            return command.Succeed();
        }

        public CommandResponse Handle(TestCommands.Fail command)
        {
            Interlocked.Increment(ref GotTestFailCommand);
            return command.Fail();
        }
        public CommandResponse Handle(TestCommands.Throw command)
        {
            Interlocked.Increment(ref GotTestThrowCommand);
            throw new TestException();
        }
        public CommandResponse Handle(TestCommands.WrapException command)
        {
            Interlocked.Increment(ref GotTestWrapCommand);
            try
            {
                throw new TestException();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public void Handle(AckCommand command)
        {
            Interlocked.Increment(ref GotAck);
        }

        public void Handle(CommandResponse command)
        {
            Interlocked.Increment(ref GotCommandResponse);
            switch (command)
            {
                case Success _:
                    Interlocked.Increment(ref GotSuccess);
                    if (command is TestCommands.TestResponse response)
                        Interlocked.Exchange(ref ResponseData, response.Data);
                    break;
                case Fail _:
                    Interlocked.Increment(ref GotFail);
                    if (command is Canceled)
                        Interlocked.Increment(ref GotCanceled);
                    if (command is TestCommands.FailedResponse failResponse)
                        Interlocked.Exchange(ref ResponseData, failResponse.Data);
                    break;
            }
        }

        public class TestException : Exception { }

        public void Dispose()
        {
            LocalDispatcher?.Dispose();
            RemoteDispatcher?.Dispose();
        }
    }

    // ReSharper disable once InconsistentNaming
    public class when_sending_commands_with_bus_connector :
        IClassFixture<TestConnectedBusFixture>

    {
        private readonly TestConnectedBusFixture _fixture;

        public when_sending_commands_with_bus_connector(TestConnectedBusFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public void can_handle_event_twice()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            long processedEvt = 0;

            _fixture.RemoteDispatcher.Subscribe(
                new AdHocHandler<TestEvent>(handler => Interlocked.Increment(ref processedEvt)));

            _fixture.RemoteDispatcher.Publish(new TestEvent(Guid.NewGuid(), Guid.NewGuid()));

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref processedEvt) == 1,
                msg: "Expected event not handled once or less, actual " + processedEvt);
            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref _fixture.GotEvent) == 1,
                msg: "Expected event not handled once or less, actual " + Interlocked.Read(ref _fixture.GotEvent));
        }

        [Fact]
        public void publish_publishes_command_as_message()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            _fixture.LocalDispatcher.Publish(new TestCommands.LocallyHandled(Guid.NewGuid(), null));
            SpinWait.SpinUntil(() => Interlocked.Read(ref _fixture.GotLocallyHandled) == 1, 250);
        }

        [Fact]
        public void fire_publishes_command_as_message()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();
            _fixture.LocalDispatcher.Fire(new TestCommands.LocallyHandled(Guid.NewGuid(), null));
            SpinWait.SpinUntil(() => Interlocked.Read(ref _fixture.GotLocallyHandled) == 1, 250);
        }

        [Fact]
        public void tryfire_publishes_command_as_message()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();
            _fixture.LocalDispatcher.TryFire(new TestCommands.LocallyHandled(Guid.NewGuid(), null));
            SpinWait.SpinUntil(() => Interlocked.Read(ref _fixture.GotLocallyHandled) == 1, 250);
        }

        [Fact]
        public void firing_commands_on_connected_buses_should_succeed()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            // subscription on local bus
            _fixture.RemoteDispatcher.Fire(new TestCommands.LocallyHandled(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(() => Interlocked.Read(ref _fixture.GotLocallyHandled) == 1, msg: "Expected LocallyHandled to be handled");

            // subscription on remote bus
            _fixture.LocalDispatcher.Fire(new TestCommands.RemoteHandled(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(() => Interlocked.Read(ref _fixture.GotRemoteHandled) == 1, msg: "Expected RemoteHandled to be handled");
        }

        [Fact]
        public void fire_oversubscribed_commands_should_throw_oversubscribed()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            long processedCmd = 0;


            _fixture.RemoteDispatcher.Subscribe(new AdHocCommandHandler<TestCommands.LocallyHandled>(
                  cmd =>
                  {
                      Interlocked.Increment(ref processedCmd);
                      Task.Delay(100).Wait();
                      return true;
                  }));

            // this works - should fail! (see below)
            //_fixture.LocalDispatcher.Fire(new TestCommands.LocallyHandled(Guid.NewGuid(), null));

            Assert.Throws<CommandOversubscribedException>(() =>
                _fixture.LocalDispatcher.Fire(new TestCommands.LocallyHandled(Guid.NewGuid(), null)));

            Assert.IsOrBecomesTrue(
                        () => Interlocked.Read(ref _fixture.GotAck) == 2,
                        msg: "Expected command Acked twice, got " + _fixture.GotAck);
            Assert.IsOrBecomesTrue(
                        () => Interlocked.Read(ref _fixture.GotLocallyHandled) == 1,
                        msg: "Expected command handled once or less, actual " + _fixture.GotLocallyHandled);


            Assert.IsOrBecomesTrue(
                        () => Interlocked.Read(ref processedCmd) == 1,
                        msg: "Expected command handled once or less, actual " + processedCmd);
        }

        [Fact]
        public void cannot_subscribe_twice_on_same_bus()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();
            Assert.Throws<ExistingHandlerException>(
                () => _fixture.LocalDispatcher.Subscribe(new AdHocCommandHandler<TestCommands.LocallyHandled>(cmd => true)));
        }

        [Fact]
        public void oversubscription_throws_existing_first_handle_succeeds()
        {
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            long processedCmd = 0;
            long gotAck = 0;

            // additional subscription on same bus is an exception
            Assert.Throws<ExistingHandlerException>(() =>
                    _fixture.LocalDispatcher.Subscribe(new AdHocCommandHandler<TestCommands.LocallyHandled>(
                         cmd =>
                         {
                             Interlocked.Increment(ref processedCmd);
                             Task.Delay(1000).Wait();
                             return true;
                         })));


            _fixture.RemoteDispatcher.Subscribe(
                        new AdHocHandler<AckCommand>(cmd => Interlocked.Increment(ref gotAck)));

            // fire on second bus is handled on first bus
            _fixture.RemoteDispatcher.Fire(new TestCommands.LocallyHandled(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(
                        () => Interlocked.Read(ref gotAck) == 1,
                        msg: "Expected command Acked handled once, got " + gotAck);

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref processedCmd) == 0,
                msg: $"Expected command TestCommands.LocallyHandled not handled, got {Interlocked.Read(ref processedCmd)}" );
        }

        [Fact]
        public void unsubscribe_removes_correct_handler()
        {
            long cmdProccessedOnRemote = 0;
            Assert.IsOrBecomesTrue(() => _fixture.LocalDispatcher.Idle);
            _fixture.ClearCounters();

            Assert.False(_fixture.LocalDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());
            var localSubscription = _fixture.LocalDispatcher.Subscribe<TestCommands.TempSubscribed>(_fixture);
            Assert.True(_fixture.LocalDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());

            _fixture.LocalDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref _fixture.GotTestTempSubscribed) == 1,
                msg: "Expected command handled once, got" + Interlocked.Read(ref _fixture.GotTestTempSubscribed));

            Interlocked.Exchange(ref _fixture.GotTestTempSubscribed, 0);

            _fixture.RemoteDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref _fixture.GotTestTempSubscribed) == 1,
                msg: "Expected command handled once, got" + Interlocked.Read(ref _fixture.GotTestTempSubscribed));

            var remoteSubscription = _fixture.RemoteDispatcher.Subscribe<TestCommands.TempSubscribed>(
                                                            new AdHocCommandHandler<TestCommands.TempSubscribed>(
                                                                cmd =>
                                                                {
                                                                    Interlocked.Increment(ref cmdProccessedOnRemote);
                                                                    return true;
                                                                }));

            Assert.True(_fixture.RemoteDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());

            _fixture.RemoteDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref cmdProccessedOnRemote) == 1,
                msg: "Expected command handled once, got" + Interlocked.Read(ref cmdProccessedOnRemote));

            localSubscription.Dispose();
            Assert.False(_fixture.LocalDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());
            Assert.True(_fixture.RemoteDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());

            _fixture.LocalDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null));

            Assert.IsOrBecomesTrue(
                () => Interlocked.Read(ref cmdProccessedOnRemote) == 2,
                msg: "Expected command handled once, got" + Interlocked.Read(ref cmdProccessedOnRemote));

            //_fixture.RemoteDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null));

            //Assert.IsOrBecomesTrue(
            //    () => Interlocked.Read(ref processedCmd) == 3,
            //    msg: "Expected command handled once, got" + Interlocked.Read(ref processedCmd));

            remoteSubscription.Dispose();
            Assert.False(_fixture.RemoteDispatcher.HasSubscriberFor<TestCommands.TempSubscribed>());

            Assert.Throws<CommandNotHandledException>(() => _fixture.LocalDispatcher.Fire(new TestCommands.TempSubscribed(Guid.NewGuid(), null)));
        }
    }
}

