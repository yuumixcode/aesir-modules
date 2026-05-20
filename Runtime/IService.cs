using System;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Business-logic layer in MVP. Encapsulates algorithms and orchestrates
    /// Models. Replaces QFramework's ISystem with standard .NET naming.
    /// </summary>
    public interface IService : IContainerAccess, IInitializable
    {
    }

    /// <summary>
    /// Base implementation of <see cref="IService"/>.
    /// Override <see cref="OnInit"/> for initialisation logic.
    /// </summary>
    public abstract class ServiceBase : IService
    {
        private IAppContext mContext;

        IAppContext IContainerAccess.Context => mContext;

        void IContainerAccess.SetContext(IAppContext context) => mContext = context;

        public bool Initialized { get; set; }
        void IInitializable.Init() => OnInit();
        public void Deinit() => OnDeinit();

        protected virtual void OnDeinit() { }
        protected abstract void OnInit();

        // Convenience accessors (same scope as QF ISystem: Model + Service + Provider + Events)
        public T GetModel<T>() where T : class, IModel => mContext.GetModel<T>();
        public T GetService<T>() where T : class, IService => mContext.GetService<T>();
        public T GetProvider<T>() where T : class, IProvider => mContext.GetProvider<T>();
        public void Invoke<TEvent>() where TEvent : new() => mContext.Invoke<TEvent>();
        public void Invoke<TEvent>(TEvent e) => mContext.Invoke(e);
        public ISubscription AddListener<TEvent>(Action<TEvent> onEvent) => mContext.AddListener(onEvent);
    }
}
