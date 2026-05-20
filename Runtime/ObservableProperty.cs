using System;
using UnityEngine;

namespace RunLab.AesirArchitecture
{
    /// <summary>
    /// Writable reactive property. Replaces QFramework's BindableProperty
    /// with naming closer to .NET's IObservable pattern.
    /// </summary>
    public interface IObservableProperty<T> : IReadOnlyObservableProperty<T>
    {
        new T Value { get; set; }
        void SetValueSilently(T value);
    }

    /// <summary>
    /// Read-only view of a reactive property.
    /// </summary>
    public interface IReadOnlyObservableProperty<T> : IEvent
    {
        T Value { get; }

        ISubscription AddListener(Action<T> onChanged);
        ISubscription AddListenerWithInit(Action<T> onChanged);
        void RemoveListener(Action<T> onChanged);
    }

    public class ObservableProperty<T> : IObservableProperty<T>
    {
        private T mValue;
        private readonly Event<T> mChanged = new Event<T>();

        public static Func<T, T, bool> Comparer { get; set; } = DefaultEquals;

        private static bool DefaultEquals(T a, T b) => a?.Equals(b) ?? b is null;

        public ObservableProperty(T defaultValue = default) => mValue = defaultValue;

        public ObservableProperty<T> WithComparer(Func<T, T, bool> comparer)
        {
            Comparer = comparer;
            return this;
        }

        public T Value
        {
            get => GetValue();
            set
            {
                if (value is null && mValue is null) return;
                if (value is not null && Comparer(value, mValue)) return;

                SetValue(value);
                mChanged.Trigger(value);
            }
        }

        protected virtual void SetValue(T value) => mValue = value;
        protected virtual T GetValue() => mValue;

        public void SetValueSilently(T value) => mValue = value;

        public ISubscription AddListener(Action<T> onChanged) => mChanged.AddListener(onChanged);

        public ISubscription AddListenerWithInit(Action<T> onChanged)
        {
            onChanged(mValue);
            return AddListener(onChanged);
        }

        public void RemoveListener(Action<T> onChanged) => mChanged.RemoveListener(onChanged);

        ISubscription IEvent.AddListener(Action onEvent)
        {
            return AddListener(Action);
            void Action(T _) => onEvent();
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal sealed class ObservablePropertyComparerAutoRegister
    {
#if UNITY_5_6_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoRegister()
        {
            ObservableProperty<int>.Comparer = (a, b) => a == b;
            ObservableProperty<float>.Comparer = (a, b) => a == b;
            ObservableProperty<double>.Comparer = (a, b) => a == b;
            ObservableProperty<string>.Comparer = (a, b) => a == b;
            ObservableProperty<long>.Comparer = (a, b) => a == b;
            ObservableProperty<Vector2>.Comparer = (a, b) => a == b;
            ObservableProperty<Vector3>.Comparer = (a, b) => a == b;
            ObservableProperty<Vector4>.Comparer = (a, b) => a == b;
            ObservableProperty<Color>.Comparer = (a, b) => a == b;
            ObservableProperty<Color32>.Comparer = (a, b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            ObservableProperty<Bounds>.Comparer = (a, b) => a == b;
            ObservableProperty<Rect>.Comparer = (a, b) => a == b;
            ObservableProperty<Quaternion>.Comparer = (a, b) => a == b;
            ObservableProperty<Vector2Int>.Comparer = (a, b) => a == b;
            ObservableProperty<Vector3Int>.Comparer = (a, b) => a == b;
            ObservableProperty<BoundsInt>.Comparer = (a, b) => a == b;
            ObservableProperty<RangeInt>.Comparer = (a, b) => a.start == b.start && a.length == b.length;
            ObservableProperty<RectInt>.Comparer = (a, b) => a.Equals(b);
        }
#endif
    }
}
