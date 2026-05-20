namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Internal contract for objects that belong to an <see cref="IAppContext"/>.
    /// Replaces QFramework's IBelongToArchitecture + ICanSetArchitecture pair
    /// with a single, simpler interface.
    /// </summary>
    public interface IContainerAccess
    {
        IAppContext Context { get; }
        void SetContext(IAppContext context);
    }

    /// <summary>
    /// Lifecycle contract for objects that require initialisation / teardown.
    /// </summary>
    public interface IInitializable
    {
        bool Initialized { get; set; }
        void Init();
        void Deinit();
    }
}
