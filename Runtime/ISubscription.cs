using System;
using System.Collections.Generic;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Handle returned from <see cref="EventBus.AddListener{T}"/>. Calling
    /// <see cref="IDisposable.Dispose"/> removes the listener. Replaces
    /// QFramework's IUnRegister with standard .NET disposable semantics.
    /// </summary>
    public interface ISubscription : IDisposable
    {
    }

    /// <summary>
    /// Collections of <see cref="ISubscription"/> that can be bulk-disposed.
    /// Replaces QFramework's IUnRegisterList.
    /// </summary>
    public interface ISubscriptionCollection
    {
        List<ISubscription> Subscriptions { get; }
    }

    public static class SubscriptionCollectionExtensions
    {
        public static void AddTo(this ISubscription subscription, ISubscriptionCollection collection) =>
            collection.Subscriptions.Add(subscription);

        public static void DisposeAll(this ISubscriptionCollection self)
        {
            foreach (var sub in self.Subscriptions)
                sub.Dispose();
            self.Subscriptions.Clear();
        }
    }

    internal sealed class DelegateSubscription : ISubscription
    {
        private Action mRemove;
        private bool mDisposed;

        public DelegateSubscription(Action remove) => mRemove = remove;

        public void Dispose()
        {
            if (mDisposed) return;
            mDisposed = true;
            mRemove?.Invoke();
            mRemove = null;
        }
    }

    /// <summary>
    /// Default <see cref="ISubscriptionCollection"/> implementation.
    /// </summary>
    public class SubscriptionList : ISubscriptionCollection
    {
        public List<ISubscription> Subscriptions { get; } = new List<ISubscription>();
    }
}
