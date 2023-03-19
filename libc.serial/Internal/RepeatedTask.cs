using System;

namespace libc.serial.Internal
{
  internal class RepeatedTask
  {
    private readonly Action _job;
    private readonly DelayedTask _task;

    public RepeatedTask(Action action, int sleepTime)
    {
      _job = action;
      _task = new DelayedTask(main, sleepTime);
    }

    public bool IsRunning { get; private set; }

    public void Start()
    {
      IsRunning = true;
      _task.Start();
    }

    public void Stop()
    {
      IsRunning = false;
      _task.Stop();
    }

    private void main()
    {
      try
      {
        _job();
      }
      catch
      {
        // ignored
      }

      if (IsRunning) _task.Start();
    }
  }
}