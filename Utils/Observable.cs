using System;

namespace AlbionNavigator.Utils;

public class Observable<T>
{
    private T _value;
    public event Action Changed;
    public event Action<T> ChangedTo;
    
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            ChangedTo?.Invoke(value);
            Changed?.Invoke();
        }
    }
}