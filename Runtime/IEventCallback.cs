using System;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Convenience interface for objects that handle a specific event type.
    /// Replaces QFramework's IOnEvent&lt;T&gt;.
    /// Named after Unity's EventCallback&lt;TEventType&gt; pattern.
    /// </summary>
    public interface IEventCallback<T>
    {
        void OnEvent(T e);
    }

    public static class EventCallbackExtensions
    {
        public static ISubscription AddListener<T>(this IEventCallback<T> self) where T : struct =>
            EventBus.Global.AddListener<T>(self.OnEvent);

        public static void RemoveListener<T>(this IEventCallback<T> self) where T : struct =>
            EventBus.Global.RemoveListener<T>(self.OnEvent);
    }
}
