﻿using System;
using System.Threading;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.Messaging
{
    public class Command : Message, ICorrelatedMessage
    {
        public Guid? SourceId { get; }
        public Guid CorrelationId { get; }

        public bool TimeoutTcpWait = true;

        public Command(Guid correlationId, Guid? sourceId)
        {
            CorrelationId = correlationId;
            SourceId = sourceId;
        }

        public CommandResponse Succeed()
        {
            return new Success(this);
        }

        public CommandResponse Fail(Exception ex = null)
        {
            return new Fail(this, ex);
        }
      }
    /// <summary>
    /// Indicates receipt of a command message
    /// Does not indicate success or failure of command processing
    /// </summary>
    public class AckCommand : Message, ICorrelatedMessage
    {
        public Command SourceCommand { get; }
        public Guid? SourceId => SourceCommand.MsgId;
        public Guid CorrelationId => SourceCommand.CorrelationId;
        public Guid CommandId => SourceCommand.MsgId;
        public Type CommandType => SourceCommand.GetType();

        public AckCommand(Command sourceCommand)
        {
            SourceCommand = sourceCommand;
        }
    }
}
