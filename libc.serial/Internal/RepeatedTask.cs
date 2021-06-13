using System;

namespace libc.serial.Internal
{
    internal class RepeatedTask
    {
        private readonly Action job;
        private readonly DelayedTask task;

        public RepeatedTask(Action action, int sleepTime)
        {
            job = action;
            task = new DelayedTask(main, sleepTime);
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
            task.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            task.Stop();
        }

        private void main()
        {
            try
            {
                job();
            }
            catch
            {
                // ignored
            }

            if (IsRunning) task.Start();
        }
    }
}