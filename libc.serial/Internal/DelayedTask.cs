using System;
using System.Timers;

namespace libc.serial.Internal
{
  internal class DelayedTask : IDisposable
  {
    private readonly Action _action;
    private readonly Timer _timer;
    private double _duration;

    public DelayedTask(Action callback, int duration)
    {
      _action = callback;
      _duration = duration;

      _timer = new Timer
      {
        AutoReset = false
      };

      _timer.Elapsed += tmr_Elapsed;
    }

    public double Duration
    {
      get => _duration;
      set
      {
        _duration = value;
        if (_duration > 0) _timer.Interval = value;
      }
    }

    public void Dispose()
    {
      Stop();
      _timer?.Dispose();
    }

    public void Start()
    {
      if (Math.Abs(Duration) < 0.0009)
      {
        _action.BeginInvoke(null, null);
      }
      else
      {
        _timer.Interval = Duration;
        _timer.Start();
      }
    }

    public void Stop()
    {
      try
      {
        _timer.Stop();
      }
      catch
      {
        // ignored
      }
    }

    public void Reset()
    {
      Stop();
      Start();
    }

    private void tmr_Elapsed(object sender, ElapsedEventArgs e)
    {
      _action();
    }
  }
}