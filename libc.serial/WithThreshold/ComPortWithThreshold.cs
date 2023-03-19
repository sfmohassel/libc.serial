using libc.serial.Base;
using libc.serial.Internal;
using System;
using System.Collections.Generic;

namespace libc.serial.WithThreshold
{
  public class ComPortWithThreshold : ComPort
  {
    private readonly RepeatedTask _checkBufferThread;
    private readonly Func<List<byte>, bool> _packetRcv;
    private readonly ComPortWithThresholdSettings _settings;

    public ComPortWithThreshold(ComPortWithThresholdSettings settings,
        Func<List<byte>, bool> packetRcvAction,
        Action<ComPortErrorNames, Exception> errorCallback)
        : base(settings, errorCallback)
    {
      try
      {
        this._settings = settings;
        _packetRcv = packetRcvAction ?? throw new ArgumentNullException(nameof(packetRcvAction));
        Buffer = new Queue<byte>();
        _checkBufferThread = new RepeatedTask(checkBuffer, 1);
        _checkBufferThread.Start();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnConstruction, ex);
      }
    }

    public override void Dispose()
    {
      base.Dispose();

      try
      {
        _checkBufferThread.Stop();
        clearBuffer();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnDispose, ex);
      }
    }

    private void checkBuffer()
    {
      List<byte> packet;
      var count = _settings.Threshold;

      lock (Buffer)
      {
        if (Buffer.Count < count) return;
        packet = new List<byte>();
        for (var i = 0; i < count; i++) packet.Add(Buffer.Dequeue());
      }

      var valid = _packetRcv(packet);
      if (!valid) clearBuffer();
    }

    private void clearBuffer()
    {
      TryClearInBuffer();

      lock (Buffer)
      {
        Buffer.Clear();
      }
    }
  }
}