using libc.serial.AtDevice;
using libc.serial.Internal;

namespace libc.serial.Sim900
{
    public class AtCTextFormat : AtCommand
    {
        private const string cmd = "at+cmgf=1\r";

        public override string ToString()
        {
            return $"Command: {cmd} - State: {State}";
        }

        protected override void RunCommandAsync()
        {
            port.Write(cmd);
            setTimeout(1000);
        }

        protected override void OnDataUpdate(string buffer)
        {
            buffer = buffer.StartingFrom(cmd);

            if (buffer == null) return;
            var bOk = buffer.GetBetween(cmd, Ok);
            var bError = buffer.GetBetween(cmd, Error);

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