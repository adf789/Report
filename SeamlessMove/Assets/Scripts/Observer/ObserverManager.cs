
using System;
using System.Collections.Generic;

public class ObserverManager
{
    private static ObserverManager _instance;
    public static ObserverManager Instance => _instance ?? (_instance = new ObserverManager());
    private Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

    public void AddObserver<T>(Action<T> onEvent) where T : IObserverParam
    {
        Type type = typeof(T);

        if (_events.TryGetValue(type, out var existingDelegate))
        {
            _events[type] = Delegate.Combine(existingDelegate, onEvent);
        }
        else
        {
            _events[type] = onEvent;
        }
    }

    public void RemoveObserver<T>(Action<T> onEvent) where T : IObserverParam
    {
        Type type = typeof(T);

        if (_events.TryGetValue(type, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, onEvent);

            if (newDelegate == null)
                _events.Remove(type);
            else
                _events[type] = newDelegate;
        }
    }

    public void NotifyObserver<T>(T param) where T : IObserverParam
    {
        if (param == null)
            return;

        Type type = param.GetType();

        if (_events.TryGetValue(type, out var eventDelegate))
        {
            (eventDelegate as Action<T>)?.Invoke(param);
        }
    }
}
