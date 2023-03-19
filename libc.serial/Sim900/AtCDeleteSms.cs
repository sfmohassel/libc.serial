using libc.serial.AtDevice;
using libc.serial.Internal;

namespace libc.serial.Sim900
{
  public class AtCDeleteSms : AtCommand
  {
    private readonly string _cmd;

    public AtCDeleteSms(int index, AtCDeleteFlags flag)
    {
      _cmd = $"at+cmgd={index},{(int)flag}\r";
    }

    public AtCDeleteSms(AtCDeleteFlags flag)
        : this(1, flag)
    {
    }

    public override string ToString()
    {
      return $"Command: {_cmd} - State: {State}";
    }

    protected override void RunCommandAsync()
    {
      port.Write(_cmd);
      setTimeout(10000);
    }

    protected override void OnDataUpdate(string buffer)
    {
      buffer = buffer.StartingFrom(_cmd);

      if (buffer == null) return;
      var bOk = buffer.GetBetween(_cmd, Ok);
      var bError = buffer.GetBetween(_cmd, Error);

      if (bOk != null)
      {
        AddResponse(Ok);
        finish(true, AtCommandStates.Completed);
      }
      else if (bError != null)
      {
        AddResponse(Error);
        finish(false, AtCommandStates.Completed);
      }
    }
  }
}