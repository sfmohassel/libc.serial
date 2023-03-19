using libc.serial.Internal;
using libc.serial.Resources;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace libc.serial.Base
{
  public class ComPort : IComPort
  {
    protected readonly Action<ComPortErrorNames, Exception> Error;
    private readonly DelayedTask _mainTask;
    private readonly object _mustBeOpenLock = new object();
    protected readonly Action<byte> NewByteCallback;
    private readonly SerialPort _port;
    protected Queue<byte> Buffer;
    private bool _mustBeOpen;
    private int _openErrorCount = -1;

    public ComPort(ComPortSettings settings, Action<byte> newByteCallback,
        Action<ComPortErrorNames, Exception> errorCallback)
        : this(settings, errorCallback)
    {
      NewByteCallback = newByteCallback ?? throw new ArgumentNullException(nameof(newByteCallback));
    }

    protected ComPort(ComPortSettings settings, Action<ComPortErrorNames, Exception> errorCallback)
    {
      try
      {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        if (settings.Validate() == false)
          throw new ArgumentException(Tran.Instance.Get("InvalidSettings"), nameof(settings));

        Error = errorCallback ?? throw new ArgumentNullException(nameof(errorCallback));

        _port = new SerialPort
        {
          PortName = settings.PortName,
          BaudRate = settings.BaudRate,
          DataBits = settings.DataBits,
          Parity = settings.Parity,
          StopBits = settings.StopBits,
          Handshake = settings.Handshake,
          ReadBufferSize = settings.ReadBufferSize,
          WriteBufferSize = settings.WriteBufferSize
        };
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnConstruction, ex);
      }

      try
      {
        _mainTask = new DelayedTask(main, 1);
        _mainTask.Start();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnStartingReadOperation, ex);
      }
    }

    public bool IsOpen => _port != null && _port.IsOpen;

    public event Action OnOpen,
        OnClose;

    public void TryClearInBuffer()
    {
      try
      {
        if (IsOpen) _port.DiscardInBuffer();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnClearInBuffer, ex);
      }
    }

    public void TryClearOutBuffer()
    {
      try
      {
        if (IsOpen) _port.DiscardOutBuffer();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnClearOutBuffer, ex);
      }
    }

    public void Open()
    {
      lock (_mustBeOpenLock)
      {
        _mustBeOpen = true;
      }
    }

    public void Close()
    {
      lock (_mustBeOpenLock)
      {
        _mustBeOpen = false;
      }
    }

    public virtual void Write(string data, bool flush = false)
    {
      try
      {
        if (!IsOpen) Thread.Sleep(5);

        if (IsOpen)
        {
          _port.Write(data);
          if (flush) _port.BaseStream.Flush();
        }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnWrite, ex);
      }
    }

    public virtual void Write(IEnumerable<byte> data, bool flush = false)
    {
      try
      {
        if (!IsOpen) Thread.Sleep(5);

        if (IsOpen)
        {
          var arr = data.ToArray();
          _port.Write(arr, 0, arr.Length);
          if (flush) _port.BaseStream.Flush();
        }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnWrite, ex);
      }
    }

    public virtual void Dispose()
    {
      try
      {
        _mainTask.Stop();
        _port.Dispose();
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnDispose, ex);
      }
    }

    protected void raiseError(ComPortErrorNames errorName, Exception ex)
    {
      if (errorName == ComPortErrorNames.OnOpen)
        ++_openErrorCount;
      else
        _openErrorCount = 0;

      if (_openErrorCount >= 2)
      {
        _openErrorCount = 2;

        return;
      }

      Error?.BeginInvoke(errorName, ex, null, null);
    }

    private void open()
    {
      try
      {
        if (!IsOpen)
        {
          _port.Open();
          if (IsOpen) OnOpen?.BeginInvoke(null, null);
        }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnOpen, ex);
      }
    }

    private void close()
    {
      try
      {
        if (IsOpen)
        {
          _port.Close();
          if (!IsOpen) OnClose?.BeginInvoke(null, null);
        }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnClose, ex);
      }
    }

    private void main()
    {
      try
      {
        bool beOpen;

        lock (_mustBeOpenLock)
        {
          beOpen = _mustBeOpen;
        }

        if (beOpen)
        {
          if (IsOpen)
          {
            if (NewByteCallback != null)
              read_NewByteCallback();
            else if (Buffer != null) read_Buffer();
          }
          else
          {
            open();
          }
        }
        else
        {
          close();
        }
      }
      finally
      {
        _mainTask.Start();
      }
    }

    private void read_NewByteCallback()
    {
      try
      {
        var count = _port.BytesToRead;

        if (count > 0)
          for (var i = 0; i < count; i++)
          {
            //check if the port needs to be closed right away
            if (_mustBeOpen == false) break;
            var temp = new byte[1];
            if (_port.Read(temp, 0, temp.Length) == 1) NewByteCallback(temp[0]);
          }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnRead, ex);
      }
    }

    private void read_Buffer()
    {
      try
      {
        var count = _port.BytesToRead;

        if (count > 0)
        {
          var arr = new byte[count];
          var bytesRead = _port.Read(arr, 0, arr.Length);

          if (bytesRead > 0)
          {
            var items = arr.Take(bytesRead).ToList();

            lock (Buffer)
            {
              foreach (var item in items) Buffer.Enqueue(item);
            }
          }
        }
      }
      catch (Exception ex)
      {
        raiseError(ComPortErrorNames.OnRead, ex);
      }
    }
  }
}