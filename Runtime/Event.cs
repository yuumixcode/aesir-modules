using System;
using System.Collections.Generic;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Minimal event interface for type-erased access.
    /// </summary>
    public interface IEvent
    {
        ISubscription AddListener(Action onEvent);
    }

    public class Event : IEvent
    {
        private Action mHandler = () => { };

        public ISubscription AddListener(Action onEvent)
        {
            mHandler += onEvent;
            return new DelegateSubscription(() => RemoveListener(onEvent));
        }

        public ISubscription AddListenerWithInvoke(Action onEvent)
        {
            onEvent.Invoke();
            return AddListener(onEvent);
        }

        public void RemoveListener(Action onEvent) => mHandler -= onEvent;

        public void Invoke() => mHandler?.Invoke();
    }

    public class Event<T> : IEvent
    {
        private Action<T> mHandler = _ => { };

        public ISubscription AddListener(Action<T> onEvent)
        {
            mHandler += onEvent;
            return new DelegateSubscription(() => RemoveListener(onEvent));
        }

        public void RemoveListener(Action<T> onEvent) => mHandler -= onEvent;

        public void Trigger(T t) => mHandler?.Invoke(t);

        ISubscription IEvent.AddListener(Action onEvent)
        {
            return AddListener(Action);
            void Action(T _) => onEvent();
        }
    }

    public class Event<T1, T2> : IEvent
    {
        private Action<T1, T2> mHandler = (_, _) => { };

        public ISubscription AddListener(Action<T1, T2> onEvent)
        {
            mHandler += onEvent;
            return new DelegateSubscription(() => RemoveListener(onEvent));
        }

        public void RemoveListener(Action<T1, T2> onEvent) => mHandler -= onEvent;

        public void Trigger(T1 t1, T2 t2) => mHandler?.Invoke(t1, t2);

        ISubscription IEvent.AddListener(Action onEvent)
        {
            return AddListener(Action);
            void Action(T1 _, T2 __) => onEvent();
        }
    }

    public class Event<T1, T2, T3> : IEvent
    {
        private Action<T1, T2, T3> mHandler = (_, _, _) => { };

        public ISubscription AddListener(Action<T1, T2, T3> onEvent)
        {
            mHandler += onEvent;
            return new DelegateSubscription(() => RemoveListener(onEvent));
        }

        public void RemoveListener(Action<T1, T2, T3> onEvent) => mHandler -= onEvent;

        public void Trigger(T1 t1, T2 t2, T3 t3) => mHandler?.Invoke(t1, t2, t3);

        ISubscription IEvent.AddListener(Action onEvent)
        {
            return AddListener(Action);
            void Action(T1 _, T2 __, T3 ___) => onEvent();
        }
    }

    /// <summary>
    /// Type-keyed event storage. Replaces QFramework's EasyEvents.
    /// </summary>
    public class EventCollection
    {
        private readonly Dictionary<Type, IEvent> mEvents = new Dictionary<Type, IEvent>();

        public T Get<T>() where T : IEvent =>
            mEvents.TryGetValue(typeof(T), out var e) ? (T)e : default;

        public T GetOrAdd<T>() where T : IEvent, new()
        {
            var type = typeof(T);
            if (mEvents.TryGetValue(type, out var e))
                return (T)e;

            var t = new T();
            mEvents.Add(type, t);
            return t;
        }
    }
}
