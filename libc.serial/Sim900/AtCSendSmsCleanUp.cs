using libc.models.Extensions;
using libc.serial.AtDevice;
namespace libc.serial.Sim900 {
    public class AtCSendSmsCleanUp : AtCommand {
        private readonly string cmd = $"{(char) 27}at\r";
        protected override void RunCommandAsync() {
            port.Write(cmd);
            setTimeout(4000);
        }
        protected override void OnDataUpdate(string buffer) {
            buffer = buffer.StartingFrom(cmd);
            if (buffer == null) return;
            var bOk = buffer.GetBetween(cmd, Ok);
            var bError = buffer.GetBetween(cmd, Error);
            if (bOk != null) {
                AddResponse(Ok);
                finish(true, AtCommandStates.Completed);
            } else if (bError != null) {
                AddResponse(Error);
                finish(false, AtCommandStates.Completed);
            }
        }
    }
}