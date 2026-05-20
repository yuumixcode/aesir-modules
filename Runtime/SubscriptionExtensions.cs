using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunLab.AesirArchitecture
{
#if UNITY_5_6_OR_NEWER
    public abstract class SubscriptionTrigger : MonoBehaviour
    {
        private readonly HashSet<ISubscription> mSubscriptions = new HashSet<ISubscription>();

        public ISubscription Add(ISubscription subscription)
        {
            mSubscriptions.Add(subscription);
            return subscription;
        }

        public void Remove(ISubscription subscription) => mSubscriptions.Remove(subscription);

        protected void DisposeAll()
        {
            foreach (var sub in mSubscriptions)
                sub.Dispose();
            mSubscriptions.Clear();
        }
    }

    public class SubscriptionOnDestroyTrigger : SubscriptionTrigger
    {
        private void OnDestroy() => DisposeAll();
    }

    public class SubscriptionOnDisableTrigger : SubscriptionTrigger
    {
        private void OnDisable() => DisposeAll();
    }

    public class SubscriptionOnSceneUnloadedTrigger : SubscriptionTrigger
    {
        private static SubscriptionOnSceneUnloadedTrigger mDefault;

        public static SubscriptionOnSceneUnloadedTrigger Default
        {
            get
            {
                if (!mDefault)
                {
                    mDefault = new GameObject(nameof(SubscriptionOnSceneUnloadedTrigger))
                        .AddComponent<SubscriptionOnSceneUnloadedTrigger>();
                }
                return mDefault;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            hideFlags = HideFlags.HideInHierarchy;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy() => SceneManager.sceneUnloaded -= OnSceneUnloaded;
        private void OnSceneUnloaded(Scene scene) => DisposeAll();
    }
#endif

    public static class SubscriptionExtensions
    {
#if UNITY_5_6_OR_NEWER
        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c ? c : go.AddComponent<T>();
        }

        /// <summary>
        /// Auto-dispose the subscription when the GameObject is destroyed.
        /// </summary>
        public static ISubscription DisposeWhenDestroyed(this ISubscription self, GameObject go) =>
            GetOrAddComponent<SubscriptionOnDestroyTrigger>(go).Add(self);

        /// <summary>
        /// Auto-dispose the subscription when the Component's GameObject is destroyed.
        /// </summary>
        public static ISubscription DisposeWhenDestroyed<T>(this ISubscription self, T component)
            where T : Component =>
            self.DisposeWhenDestroyed(component.gameObject);

        /// <summary>
        /// Auto-dispose the subscription when the GameObject is disabled.
        /// </summary>
        public static ISubscription DisposeWhenDisabled(this ISubscription self, GameObject go) =>
            GetOrAddComponent<SubscriptionOnDisableTrigger>(go).Add(self);

        /// <summary>
        /// Auto-dispose the subscription when the Component is disabled.
        /// </summary>
        public static ISubscription DisposeWhenDisabled<T>(this ISubscription self, T component)
            where T : Component =>
            self.DisposeWhenDisabled(component.gameObject);

        /// <summary>
        /// Auto-dispose the subscription when the current scene is unloaded.
        /// </summary>
        public static ISubscription DisposeWhenSceneUnloaded(this ISubscription self) =>
            SubscriptionOnSceneUnloadedTrigger.Default.Add(self);
#endif
    }
}
