using System;
using System.Collections.Generic;
using System.Linq;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Minimal IoC container keyed by service type. Replaces QFramework's
    /// IOCContainer with clearer .NET-aligned naming.
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        public void Register<T>(T instance)
        {
            var key = typeof(T);
            if (mInstances.ContainsKey(key))
                mInstances[key] = instance;
            else
                mInstances.Add(key, instance);
        }

        public T Get<T>() where T : class
        {
            return mInstances.TryGetValue(typeof(T), out var instance) ? instance as T : null;
        }

        public IEnumerable<T> GetInstances<T>()
        {
            var type = typeof(T);
            return mInstances.Values.Where(i => type.IsInstanceOfType(i)).Cast<T>();
        }

        public void Clear() => mInstances.Clear();
    }
}
