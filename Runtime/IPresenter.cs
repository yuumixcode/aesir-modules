using System;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Entry point for MonoBehaviour-based UI components into the MVP framework.
    /// The Presenter mediates between the View (Unity UI) and the Model / Service layers.
    /// </summary>
    public interface IPresenter : IContainerAccess
    {
    }

    /// <summary>
    /// Convenience extension methods so any <see cref="IPresenter"/> can reach
    /// the context without casting.
    /// </summary>
    public static class PresenterExtensions
    {
        public static T GetModel<T>(this IPresenter self) where T : class, IModel =>
            self.Context.GetModel<T>();

        public static T GetService<T>(this IPresenter self) where T : class, IService =>
            self.Context.GetService<T>();

        public static T GetProvider<T>(this IPresenter self) where T : class, IProvider =>
            self.Context.GetProvider<T>();

        public static void ExecuteCommand<T>(this IPresenter self, T command) where T : ICommand =>
            self.Context.ExecuteCommand(command);

        public static void ExecuteCommand<T>(this IPresenter self) where T : ICommand, new() =>
            self.Context.ExecuteCommand(new T());

        public static TResult ExecuteCommand<TResult>(this IPresenter self, ICommand<TResult> command) =>
            self.Context.ExecuteCommand(command);

        public static void Invoke<TEvent>(this IPresenter self) where TEvent : new() =>
            self.Context.Invoke<TEvent>();

        public static void Invoke<TEvent>(this IPresenter self, TEvent e) =>
            self.Context.Invoke(e);

        public static ISubscription AddListener<TEvent>(this IPresenter self, Action<TEvent> onEvent) =>
            self.Context.AddListener(onEvent);

        public static void RemoveListener<TEvent>(this IPresenter self, Action<TEvent> onEvent) =>
            self.Context.RemoveListener(onEvent);
    }
}
