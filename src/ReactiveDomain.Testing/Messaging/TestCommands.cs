using System;
using System.Threading;

namespace ReactiveDomain.Messaging.Testing
{
    public class TestCommands
    {
        public class TimeoutTestCommand : Command
        {
            public TimeoutTestCommand(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class Fail : Command
        {
            public Fail(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class Throw : Command
        {
            public Throw(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class WrapException : Command
        {
            public WrapException(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class ChainedCaller : Command
        {
            public ChainedCaller(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class Command1 : Command
        {
            public Command1(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class Command2 : Command
        {
            public Command2(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class Command3 : Command
        {
            public Command3(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class RemoteHandled : Command
        {
            public RemoteHandled(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        //n.b. don't register a handler for this
        public class Unhandled : Command
        {
            public Unhandled(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class LongRunning : Command
        {
            public LongRunning(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class TypedResponse : Command
        {
            public readonly bool FailCommand;
            public TypedResponse(
                bool failCommand,
                Guid correlationId, 
                Guid? sourceId) : base(correlationId, sourceId) {
                FailCommand = failCommand;
            }

            public TestResponse Succeed(int data)
            {
                return new TestResponse(
                            this,
                            data);
            }
            public FailedResponse Fail(Exception ex, int data)
            {
                return new FailedResponse(
                            this,
                            ex,
                            data);
            }
        }
        public class DisjunctCommand : Command
        {
            public DisjunctCommand(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }
        public class UnsubscribedCommand : Command
        {
            public UnsubscribedCommand(Guid correlationId, Guid? sourceId) : base(correlationId, sourceId) { }
        }

        public class TestResponse : Success
        {
            public int Data { get; }

            public TestResponse(
                TypedResponse sourceCommand,
                int data) :
                    base(sourceCommand)
            {
                Data = data;
            }
        }
        public class FailedResponse : Messaging.Fail
        {
            public int Data { get; }
            public FailedResponse(
               TypedResponse sourceCommand,
               Exception exception,
               int data) :
                    base(sourceCommand, exception)
            {
                Data = data;
            }
        }
    }
}
