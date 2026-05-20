using System;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Type-based event bus using Unity-idiomatic naming:
    /// <c>AddListener</c> / <c>RemoveListener</c> / <c>Invoke</c>.
    /// Replaces QFramework's TypeEventSystem.
    ///
    /// Naming aligned with UnityCsReference:
    ///   - UnityEvent.AddListener / RemoveListener / Invoke
    ///     (Runtime/Export/UnityEvent/UnityEvent.cs)
    ///   - CallbackEventHandler.RegisterCallback / UnregisterCallback
    ///     (Modules/UIElements/Core/CallbackEventHandler.cs)
    /// </summary>
    public class EventBus
    {
        public static readonly EventBus Global = new EventBus();

        private readonly EventCollection mEvents = new EventCollection();

        public void Invoke<T>() where T : new() => mEvents.Get<Event<T>>()?.Trigger(new T());

        public void Invoke<T>(T e) => mEvents.Get<Event<T>>()?.Trigger(e);

        public ISubscription AddListener<T>(Action<T> onEvent) =>
            mEvents.GetOrAdd<Event<T>>().AddListener(onEvent);

        public void RemoveListener<T>(Action<T> onEvent)
        {
            var e = mEvents.Get<Event<T>>();
            e?.RemoveListener(onEvent);
        }
    }
}
