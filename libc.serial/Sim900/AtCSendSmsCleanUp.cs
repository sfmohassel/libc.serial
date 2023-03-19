using libc.serial.AtDevice;
using libc.serial.Internal;

namespace libc.serial.Sim900
{
  public class AtCSendSmsCleanUp : AtCommand
  {
    private readonly string _cmd = $"{(char)27}at\r";

    protected override void RunCommandAsync()
    {
      port.Write(_cmd);
      setTimeout(4000);
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