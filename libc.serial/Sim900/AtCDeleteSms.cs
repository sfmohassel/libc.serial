using libc.models.Extensions;
using libc.serial.AtDevice;
namespace libc.serial.Sim900 {
    public class AtCDeleteSms : AtCommand {
        private readonly string cmd;
        public AtCDeleteSms(int index, AtCDeleteFlags flag) {
            cmd = $"at+cmgd={index},{(int) flag}\r";
        }
        public AtCDeleteSms(AtCDeleteFlags flag)
            : this(1, flag) {
        }
        public override string ToString() {
            return $"Command: {cmd} - State: {State}";
        }
        protected override void RunCommandAsync() {
            port.Write(cmd);
            setTimeout(10000);
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