using System;
using libc.serial.AtDevice;
using libc.serial.Internal;

namespace libc.serial.Sim900
{
    public class AtCSendSms : AtCommand
    {
        private readonly string cmd;
        private readonly Func<string> getText;
        private readonly string mobile;

        public AtCSendSms(string mobile, Func<string> getTextFunc)
        {
            this.mobile = mobile.StartsWith("0") ? mobile.Substring(1) : mobile;
            getText = getTextFunc;
            cmd = $"at+cmgs=\"+98{this.mobile}\"\r";
        }

        public int OutboxIndex { get; private set; }
        public bool NeedToCleanUp { get; private set; }

        public override string ToString()
        {
            return $"Command: {cmd} - State: {State} - Outbox Index: {OutboxIndex}";
        }

        protected override void RunCommandAsync()
        {
            port.Write(cmd);
            NeedToCleanUp = true;
            setTimeout(9000);
        }

        protected override void OnDataUpdate(string buffer)
        {
            buffer = buffer.StartingFrom(cmd);

            if (buffer == null) return;

            if (buffer.EndsWith("> "))
            {
                cancelTimeout();
                port.Write(string.Format("{0}" + (char) 26, getText()));
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