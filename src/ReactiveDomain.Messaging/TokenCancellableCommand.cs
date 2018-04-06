﻿using System;
using System.Threading;
using ReactiveDomain.Messaging.Bus;

namespace ReactiveDomain.Messaging
{
    /// <summary>
    /// A command that supports cancellation via cancelation tokens
    /// n.b. this will only work inside a single process
    /// </summary>
    public abstract class TokenCancellableCommand:Command
    {
        public readonly CancellationToken CancellationToken;
        public bool IsCanceled => CancellationToken.IsCancellationRequested;

        protected TokenCancellableCommand(
            Guid correlationId,
            Guid? sourceId,
            CancellationToken cancellationToken) : 
            base(correlationId, sourceId)
        {
            if(cancellationToken == null )
                throw new ArgumentNullException(nameof(cancellationToken));
            CancellationToken = cancellationToken;
        }

     
        public CommandResponse Canceled()
        {
            return new Canceled(this);
        }

    }
    public class Canceled : Fail
    {
        public Canceled(Command sourceCommand) : base(sourceCommand,new CommandCanceledException(sourceCommand)) { }
    }
}
