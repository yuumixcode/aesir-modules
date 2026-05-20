using System;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Composition root interface. Provides dependency resolution, event bus,
    /// and command execution — the single entry point for the entire MVP graph.
    /// </summary>
    public interface IAppContext
    {
        void RegisterService<T>(T service) where T : IService;
        void RegisterModel<T>(T model) where T : IModel;
        void RegisterProvider<T>(T provider) where T : IProvider;

        T GetService<T>() where T : class, IService;
        T GetModel<T>() where T : class, IModel;
        T GetProvider<T>() where T : class, IProvider;

        void ExecuteCommand<T>(T command) where T : ICommand;
        TResult ExecuteCommand<TResult>(ICommand<TResult> command);

        void Invoke<T>() where T : new();
        void Invoke<T>(T e);

        ISubscription AddListener<T>(Action<T> onEvent);
        void RemoveListener<T>(Action<T> onEvent);

        void Deinit();
    }
}
