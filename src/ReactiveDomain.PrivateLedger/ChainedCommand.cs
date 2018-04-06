using System;
using System.Threading;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.PrivateLedger
{
    public class ChainedCommand : Command, IChainedMessage
    {
        public Guid PrincipalId { get; }
        public readonly ChainSource Source;
        public ChainedCommand(IChainedMessage source) :
            base(source.CorrelationId, source.MsgId)
        {
            PrincipalId = source.PrincipalId;
            Source = source.GetMemento();
        }

        public ChainedCommand(Guid correlationId, Guid? sourceId, Guid principalId) :
            base(correlationId, sourceId)
        {
            PrincipalId = principalId;
        }

    }
}