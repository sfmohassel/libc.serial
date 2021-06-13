using System;
using System.Threading;
using libc.serial.Base;
using libc.serial.Internal;

namespace libc.serial.AtDevice
{
    public class AtController
    {
        private readonly QueueThreadSafe<AtCommand> cmdQ;
        private readonly ComPort port;
        private volatile AtCommand activeCmd;
        private volatile bool cannotStop;
        private volatile bool isAnyCommandActive;
        private volatile bool isRunning;
        private DelayedTask sendTask;

        public AtController(ComPortSettings settings, Action<ComPortErrorNames, Exception> errorCallback)
        {
            port = new ComPort(settings, rcv, errorCallback);
            cmdQ = new QueueThreadSafe<AtCommand>();
        }

        public bool IsOpen => port != null && port.IsOpen;

        public void Start()
        {
            if (isRunning)
            {
                Stop();
                Start();
            }
            else
            {
                port.Open();
                startSending();
            }

            isRunning = true;
        }

        public void Stop()
        {
            while (cannotStop) Thread.Sleep(10);
            stopSending();
            port.Close();
            isRunning = false;
        }

        public void SendCommand(AtCommand cmd)
        {
            cmdQ.Enqueue(cmd);
        }

        private void rcv(byte val)
        {
            if (isAnyCommandActive) activeCmd.NewData((char) val);
        }

        private void sendNext()
        {
            //make sure there's some command active
            if (!isAnyCommandActive && cmdQ.Any())
                if (cmdQ.TryDequeue(out var cmd))
                {
                    setActiveCmd(cmd);

                    //cannot stop
                    disallowStop();

                    //now that there's one active command, let it do
                    activeCmd.ControlAsync(port, sendIsFinished);
                }

            sendTask.Start();
        }

        private void sendIsFinished(AtCommand cmd)
        {
            //active command is done
            releaseActiveCmd();

            //controller can stop now
            allowStop();
        }

        #region starting/stoping

        private void startSending()
        {
            sendTask = new DelayedTask(sendNext, 1000);
            sendTask.Start();
        }

        private void stopSending()
        {
            sendTask.Stop();
        }

        #endregion

        #region utility

        private void setActiveCmd(AtCommand cmd)
        {
            activeCmd = cmd;
            isAnyCommandActive = true;
        }

        private void releaseActiveCmd()
        {
            isAnyCommandActive = false;
        }

        private void disallowStop()
        {
            cannotStop = true;
        }

        private void allowStop()
        {
            cannotStop = false;
        }

        #endregion
    }
}