using System;
using System.Threading;

// ReSharper disable MemberCanBeProtected.Global
namespace ReactiveDomain.Messaging.Testing
{

    public class TestMessage : Message
    {

        public TestMessage(){}
    }
    public class TestMessage2 : Message
    {
        public TestMessage2(){}
    }
    public class TestMessage3 : Message
    {
        public TestMessage3(){}
    }

    public class ParentTestMessage : Message
    {
        public ParentTestMessage(){}
    }
    public class ChildTestMessage : ParentTestMessage
    {
        public ChildTestMessage(){}
    }
    public class GrandChildTestMessage : ChildTestMessage
    {
        public GrandChildTestMessage(){}
    }

    public class CountedTestMessage : Message
    {
        public int MessageNumber;

        public CountedTestMessage(int msgNumber)
        {
            MessageNumber = msgNumber;
        }
    }

    public class CountedEvent : DomainEvent
    {
        public int MessageNumber;

        public CountedEvent(int msgNumber,
                Guid correlationId,
                Guid sourceId)
                : base(correlationId, sourceId)
        {
            MessageNumber = msgNumber;
        }
    }

}

