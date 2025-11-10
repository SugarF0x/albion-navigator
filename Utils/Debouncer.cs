using System;
using System.Timers;
using Godot;
using Timer = System.Timers.Timer;

namespace AlbionNavigator.Utils;

public class Debouncer
{
    private readonly Timer Timer;
    private readonly int Interval;

    public Debouncer(int milliseconds)
    {
        Interval = milliseconds;
        Timer = new Timer();
        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = false;
    }

    private Action Action;

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        Action?.Invoke();
    }

    public void Debounce(Action action)
    {
        Action = action;
        Timer.Stop();
        Timer.Interval = Interval;
        Timer.Start();
    }
}