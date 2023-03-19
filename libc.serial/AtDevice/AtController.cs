using libc.serial.Base;
using libc.serial.Internal;
using System;
using System.Threading;

namespace libc.serial.AtDevice
{
  public class AtController
  {
    private readonly QueueThreadSafe<AtCommand> _cmdQ;
    private readonly ComPort _port;
    private volatile AtCommand _activeCmd;
    private volatile bool _cannotStop;
    private volatile bool _isAnyCommandActive;
    private volatile bool _isRunning;
    private DelayedTask _sendTask;

    public AtController(ComPortSettings settings, Action<ComPortErrorNames, Exception> errorCallback)
    {
      _port = new ComPort(settings, rcv, errorCallback);
      _cmdQ = new QueueThreadSafe<AtCommand>();
    }

    public bool IsOpen => _port != null && _port.IsOpen;

    public void Start()
    {
      if (_isRunning)
      {
        Stop();
        Start();
      }
      else
      {
        _port.Open();
        startSending();
      }

      _isRunning = true;
    }

    public void Stop()
    {
      while (_cannotStop) Thread.Sleep(10);
      stopSending();
      _port.Close();
      _isRunning = false;
    }

    public void SendCommand(AtCommand cmd)
    {
      _cmdQ.Enqueue(cmd);
    }

    private void rcv(byte val)
    {
      if (_isAnyCommandActive) _activeCmd.NewData((char)val);
    }

    private void sendNext()
    {
      //make sure there's some command active
      if (!_isAnyCommandActive && _cmdQ.Any())
        if (_cmdQ.TryDequeue(out var cmd))
        {
          setActiveCmd(cmd);

          //cannot stop
          disallowStop();

          //now that there's one active command, let it do
          _activeCmd.ControlAsync(_port, sendIsFinished);
        }

      _sendTask.Start();
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
      _sendTask = new DelayedTask(sendNext, 1000);
      _sendTask.Start();
    }

    private void stopSending()
    {
      _sendTask.Stop();
    }

    #endregion

    #region utility

    private void setActiveCmd(AtCommand cmd)
    {
      _activeCmd = cmd;
      _isAnyCommandActive = true;
    }

    private void releaseActiveCmd()
    {
      _isAnyCommandActive = false;
    }

    private void disallowStop()
    {
      _cannotStop = true;
    }

    private void allowStop()
    {
      _cannotStop = false;
    }

    #endregion
  }
}