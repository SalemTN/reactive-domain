﻿using System;
using System.Threading;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.PrivateLedger
{
    public class ChainedEvent : DomainEvent, IChainedMessage
    {
        public Guid PrincipalId { get; }
        public readonly ChainSource Source;

        protected ChainedEvent(IChainedMessage source) :
            base(source.CorrelationId, source.MsgId)
        {
            PrincipalId = source.PrincipalId;
            Source = source.GetMemento();
        }
        protected ChainedEvent(Guid correlationId, Guid sourceId, Guid principalId) :
            base(correlationId, sourceId)
        {
            PrincipalId = principalId;
        }
    }

   
}
