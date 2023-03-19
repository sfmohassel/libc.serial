using System;
using System.Collections.Generic;

namespace libc.serial
{
  public interface IComPort : IDisposable
  {
    bool IsOpen { get; }
    event Action OnOpen;
    event Action OnClose;

    void TryClearInBuffer();

    void TryClearOutBuffer();

    void Open();

    void Close();

    void Write(string data, bool flush = false);

    void Write(IEnumerable<byte> data, bool flush = false);
  }
}