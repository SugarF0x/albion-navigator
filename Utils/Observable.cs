using System;

namespace AlbionNavigator.Utils;

public class Observable<T>
{
    private T _value;
    public event Action<T> Changed;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            Changed?.Invoke(value);
        }
    }
}