using System.Threading;
using ReactiveDomain.Messaging.Messages;

namespace ReactiveDomain.Messaging
{
    public class Event : Message, IEvent
    {
        public Event()
        {

        }
    }
}
