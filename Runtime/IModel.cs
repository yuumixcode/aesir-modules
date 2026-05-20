namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Data + domain-logic layer in MVP. Owns state (often wrapped in
    /// <see cref="ObservableProperty{T}"/>) and invokes change events.
    /// Cannot depend on <see cref="IService"/> to keep the data layer pure.
    /// </summary>
    public interface IModel : IContainerAccess, IInitializable
    {
    }

    /// <summary>
    /// Base implementation of <see cref="IModel"/>.
    /// Override <see cref="OnInit"/> for initialisation logic.
    /// </summary>
    public abstract class ModelBase : IModel
    {
        private IAppContext mContext;

        IAppContext IContainerAccess.Context => mContext;

        void IContainerAccess.SetContext(IAppContext context) => mContext = context;

        public bool Initialized { get; set; }
        void IInitializable.Init() => OnInit();
        public void Deinit() => OnDeinit();

        protected virtual void OnDeinit() { }
        protected abstract void OnInit();

        // Convenience accessors (same scope as QF IModel: Provider + Events)
        public T GetProvider<T>() where T : class, IProvider => mContext.GetProvider<T>();
        public void Invoke<TEvent>() where TEvent : new() => mContext.Invoke<TEvent>();
        public void Invoke<TEvent>(TEvent e) => mContext.Invoke(e);
    }
}
