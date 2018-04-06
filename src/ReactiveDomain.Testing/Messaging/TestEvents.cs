using System;
using System.Threading;

// ReSharper disable MemberCanBeProtected.Global
namespace ReactiveDomain.Messaging.Testing
{

    public class TestEvent : DomainEvent
    {
        public TestEvent(Guid correlationId, Guid sourceId) : base(correlationId, sourceId) { }
    }
    public class ParentTestEvent : DomainEvent
    {
        public ParentTestEvent(Guid correlationId, Guid sourceId) : base(correlationId, sourceId) { }
    }
    public class ChildTestEvent : ParentTestEvent
    {
        public ChildTestEvent(Guid correlationId, Guid sourceId) : base(correlationId, sourceId) { }
    }
    public class GrandChildTestEvent : ChildTestEvent
    {
        public GrandChildTestEvent(Guid correlationId, Guid sourceId) : base(correlationId, sourceId) { }
    }

   
}

