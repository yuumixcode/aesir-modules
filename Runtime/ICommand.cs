namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Represents a stateless write-operation. Commands are the recommended way
    /// for Presenters to mutate Model state (keeping the Presenter thin).
    /// </summary>
    public interface ICommand : IContainerAccess
    {
        void Execute();
    }

    /// <summary>
    /// Command variant that returns a value.
    /// </summary>
    public interface ICommand<TResult> : IContainerAccess
    {
        TResult Execute();
    }

    /// <summary>
    /// Base implementation of <see cref="ICommand"/>.
    /// Override <see cref="OnExecute"/>.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        private IAppContext mContext;

        IAppContext IContainerAccess.Context => mContext;
        void IContainerAccess.SetContext(IAppContext context) => mContext = context;

        void ICommand.Execute() => OnExecute();
        protected abstract void OnExecute();

        // Convenience accessors (same scope as QF ICommand: full access)
        public T GetModel<T>() where T : class, IModel => mContext.GetModel<T>();
        public T GetService<T>() where T : class, IService => mContext.GetService<T>();
        public T GetProvider<T>() where T : class, IProvider => mContext.GetProvider<T>();
        public void ExecuteCommand<T>(T command) where T : ICommand => mContext.ExecuteCommand(command);
        public void ExecuteCommand<T>() where T : ICommand, new() => mContext.ExecuteCommand(new T());
        public TResult ExecuteCommand<TResult>(ICommand<TResult> command) => mContext.ExecuteCommand(command);
        public void Invoke<TEvent>() where TEvent : new() => mContext.Invoke<TEvent>();
        public void Invoke<TEvent>(TEvent e) => mContext.Invoke(e);
    }

    /// <summary>
    /// Base implementation of <see cref="ICommand{TResult}"/>.
    /// Override <see cref="OnExecute"/>.
    /// </summary>
    public abstract class CommandBase<TResult> : ICommand<TResult>
    {
        private IAppContext mContext;

        IAppContext IContainerAccess.Context => mContext;
        void IContainerAccess.SetContext(IAppContext context) => mContext = context;

        TResult ICommand<TResult>.Execute() => OnExecute();
        protected abstract TResult OnExecute();

        public T GetModel<T>() where T : class, IModel => mContext.GetModel<T>();
        public T GetService<T>() where T : class, IService => mContext.GetService<T>();
        public T GetProvider<T>() where T : class, IProvider => mContext.GetProvider<T>();
        public void ExecuteCommand<T>(T command) where T : ICommand => mContext.ExecuteCommand(command);
        public void ExecuteCommand<T>() where T : ICommand, new() => mContext.ExecuteCommand(new T());
        public TResult2 ExecuteCommand<TResult2>(ICommand<TResult2> command) => mContext.ExecuteCommand(command);
        public void Invoke<TEvent>() where TEvent : new() => mContext.Invoke<TEvent>();
        public void Invoke<TEvent>(TEvent e) => mContext.Invoke(e);
    }
}
