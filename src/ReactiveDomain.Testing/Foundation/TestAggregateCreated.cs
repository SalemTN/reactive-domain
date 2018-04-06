using System;
using System.Threading;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.Testing
{
    public class TestWoftamAggregateCreated: Message, ICorrelatedMessage
    {
        public TestWoftamAggregateCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        public Guid AggregateId { get; private set; }

        #region Implementation of ICorrelatedMessage

        public Guid? SourceId => null;
        public Guid CorrelationId => AggregateId;

        #endregion
    }
}