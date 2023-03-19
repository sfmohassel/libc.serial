using libc.serial.AtDevice;
using libc.serial.Internal;
using System;

namespace libc.serial.Sim900
{
  public class AtCSendSms : AtCommand
  {
    private readonly string _cmd;
    private readonly Func<string> _getText;
    private readonly string _mobile;

    public AtCSendSms(string mobile, Func<string> getTextFunc)
    {
      this._mobile = mobile.StartsWith("0") ? mobile.Substring(1) : mobile;
      _getText = getTextFunc;
      _cmd = $"at+cmgs=\"+98{this._mobile}\"\r";
    }

    public int OutboxIndex { get; private set; }
    public bool NeedToCleanUp { get; private set; }

    public override string ToString()
    {
      return $"Command: {_cmd} - State: {State} - Outbox Index: {OutboxIndex}";
    }

    protected override void RunCommandAsync()
    {
      port.Write(_cmd);
      NeedToCleanUp = true;
      setTimeout(9000);
    }

    protected override void OnDataUpdate(string buffer)
    {
      buffer = buffer.StartingFrom(_cmd);

      if (buffer == null) return;

      if (buffer.EndsWith("> "))
      {
        cancelTimeout();
        port.Write(string.Format("{0}" + (char)26, _getText()));
        setTimeout(11000);
      }
      else
      {
        var t1 = buffer.GetBetween("+CMGS: ", "\r\n");

        if (t1 != null)
        {
          if (t1.Contains(Error))
          {
            AddResponse(Error);
            NeedToCleanUp = false;
            finish(false, AtCommandStates.Completed);
          }

          if (buffer.Contains(Ok))
          {
            AddResponse(Ok);
            var t2 = buffer.GetBetween("+CMGS: ", "\r\n\r\nOK\r\n");
            if (t2 != null) OutboxIndex = int.Parse(t2);
            NeedToCleanUp = false;
            finish(true, AtCommandStates.Completed);
          }
        }
      }
    }
  }
}