using System;
using System.Threading;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.Testing
{
    public class WoftamEvent: Message, ICorrelatedMessage
    {
        public WoftamEvent(string property1, string property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        public string Property1 { get; private set; }
        public string Property2 { get; private set; }

        #region Implementation of ICorrelatedMessage
        public Guid? SourceId => null;
        public Guid CorrelationId => Guid.Empty;
        #endregion
    }
}