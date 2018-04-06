using System;
using System.Threading;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.Messaging
{
    public class DomainEvent : Event, ICorrelatedMessage
    {
        public Guid CorrelationId { get; }
     
        public Guid? SourceId { get; }

        protected DomainEvent(Guid correlationId, Guid sourceId)
        {
            CorrelationId = correlationId;
            SourceId = sourceId;
        }
    }
}
