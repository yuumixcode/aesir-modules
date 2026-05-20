using System;
using System.Collections.Generic;
using System.Linq;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Generic singleton composition root. Derive from this to define your
    /// application's wiring in <see cref="Configure"/>.
    /// </summary>
    public abstract class AppContext<T> : IAppContext where T : AppContext<T>, new()
    {
        private bool mInited;

        public static Action<T> OnRegisterPatch = _ => { };

        private static T mInstance;

        public static IAppContext Instance
        {
            get
            {
                if (mInstance == null) Init();
                return mInstance;
            }
        }

        public static void Init()
        {
            if (mInstance != null) return;

            mInstance = new T();
            mInstance.Configure();

            OnRegisterPatch?.Invoke(mInstance);

            foreach (var model in mInstance.mIoC.GetInstances<IModel>().Where(m => !m.Initialized))
            {
                model.Init();
                model.Initialized = true;
            }

            foreach (var service in mInstance.mIoC.GetInstances<IService>().Where(s => !s.Initialized))
            {
                service.Init();
                service.Initialized = true;
            }

            mInstance.mInited = true;
        }

        protected abstract void Configure();

        public void Deinit()
        {
            OnDeinit();

            foreach (var service in mIoC.GetInstances<IService>().Where(s => s.Initialized))
                service.Deinit();

            foreach (var model in mIoC.GetInstances<IModel>().Where(m => m.Initialized))
                model.Deinit();

            mIoC.Clear();
            mInstance = null;
        }

        protected virtual void OnDeinit() { }

        private readonly ServiceContainer mIoC = new ServiceContainer();

        public void RegisterService<TService>(TService service) where TService : IService
        {
            service.SetContext(this);
            mIoC.Register<TService>(service);

            if (mInited)
            {
                service.Init();
                service.Initialized = true;
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.SetContext(this);
            mIoC.Register<TModel>(model);

            if (mInited)
            {
                model.Init();
                model.Initialized = true;
            }
        }

        public void RegisterProvider<TProvider>(TProvider provider) where TProvider : IProvider =>
            mIoC.Register<TProvider>(provider);

        public TService GetService<TService>() where TService : class, IService =>
            mIoC.Get<TService>();

        public TModel GetModel<TModel>() where TModel : class, IModel =>
            mIoC.Get<TModel>();

        public TProvider GetProvider<TProvider>() where TProvider : class, IProvider =>
            mIoC.Get<TProvider>();

        public void ExecuteCommand<TCommand>(TCommand command) where TCommand : ICommand =>
            RunCommand(command);

        public TResult ExecuteCommand<TResult>(ICommand<TResult> command) =>
            RunCommand(command);

        protected virtual void RunCommand(ICommand command)
        {
            command.SetContext(this);
            command.Execute();
        }

        protected virtual TResult RunCommand<TResult>(ICommand<TResult> command)
        {
            command.SetContext(this);
            return command.Execute();
        }

        private readonly EventBus mEventBus = new EventBus();

        public void Invoke<TEvent>() where TEvent : new() => mEventBus.Invoke<TEvent>();

        public void Invoke<TEvent>(TEvent e) => mEventBus.Invoke<TEvent>(e);

        public ISubscription AddListener<TEvent>(Action<TEvent> onEvent) =>
            mEventBus.AddListener<TEvent>(onEvent);

        public void RemoveListener<TEvent>(Action<TEvent> onEvent) =>
            mEventBus.RemoveListener<TEvent>(onEvent);
    }
}
