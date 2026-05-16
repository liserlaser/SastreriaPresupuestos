using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityToolkit.Mvvm.Messaging
{
    public delegate void MessageHandler<in TMessage>(object recipient, TMessage message);

    public sealed class WeakReferenceMessenger
    {
        private readonly Dictionary<Type, List<Registration>> registrations = new();

        public static WeakReferenceMessenger Default { get; } = new();

        public void Register<TMessage>(object recipient, MessageHandler<TMessage> handler)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var messageType = typeof(TMessage);

            if (!registrations.TryGetValue(messageType, out var messageRegistrations))
            {
                messageRegistrations = new List<Registration>();
                registrations[messageType] = messageRegistrations;
            }

            messageRegistrations.Add(new Registration(
                new WeakReference(recipient),
                (target, message) => handler(target, (TMessage)message)));
        }

        public void Send<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);

            if (!registrations.TryGetValue(messageType, out var messageRegistrations))
                return;

            foreach (var registration in messageRegistrations.ToList())
            {
                var recipient = registration.Recipient.Target;

                if (recipient == null)
                {
                    messageRegistrations.Remove(registration);
                    continue;
                }

                registration.Handler(recipient, message!);
            }
        }

        public void UnregisterAll(object recipient)
        {
            foreach (var messageRegistrations in registrations.Values)
            {
                messageRegistrations.RemoveAll(registration =>
                {
                    var target = registration.Recipient.Target;
                    return target == null || ReferenceEquals(target, recipient);
                });
            }
        }

        private sealed record Registration(
            WeakReference Recipient,
            Action<object, object> Handler);
    }
}
