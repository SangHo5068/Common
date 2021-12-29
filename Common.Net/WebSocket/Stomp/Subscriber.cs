using System;

namespace Common.Net.Stomp
{
    public class Subscriber
    {
        public EventHandler<object> Handler { get; }
        public Type BodyType { get; }

        public Subscriber(EventHandler<object> handler, Type bodyType)
        {
            Handler = handler;
            BodyType = bodyType;
        }
    }
}
