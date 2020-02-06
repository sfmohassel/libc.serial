using System.Collections.Generic;
using libc.models.Extensions;
using libc.serial.AtDevice;
namespace libc.serial.Sim900 {
    public class AtCGetCharge : AtCommand {
        private static readonly IDictionary<SimCardOperators, string> getCharge = new Dictionary
            <SimCardOperators, string> {
                {
                    SimCardOperators.Irancell, $"at+cusd=1,\"{"*141*1#"}\"\r"
                }, {
                    SimCardOperators.HamrahAval, $"at+cusd=1,\"{"*140*11#"}\"\r"
                }
            };
        private readonly string cmd;
        public AtCGetCharge(SimCardOperators simOperator) {
            SimCardOperator = simOperator;
            Etebar = -1;
            cmd = getCharge[simOperator];
        }
        public int Etebar { get; private set; }
        public SimCardOperators SimCardOperator { get; }
        public override string ToString() {
            return $"Command: {cmd} - State: {State} - Etebar: {Etebar}";
        }
        protected override void RunCommandAsync() {
            port.Write(cmd);
            setTimeout(6000);
        }
        protected override void OnDataUpdate(string buffer) {
            buffer = buffer.StartingFrom(cmd);
            if (buffer == null) return;
            if (buffer.Contains(Error)) {
                AddResponse(Error);
                finish(false, AtCommandStates.Completed);
            } else if (buffer.Contains(Ok)) {
                var bCusd = buffer.GetBetween("+CUSD:", "\r\n");
                if (bCusd != null && bCusd.Contains("Etebar:") && bCusd.Contains("Rial")) {
                    Etebar = getEtebar(bCusd);
                    AddResponse(Ok);
                    finish(true, AtCommandStates.Completed);
                }
            }
        }
        private int getEtebar(string s) {
            var etebar = -1;
            var m = s.GetBetween("Etebar:", "Rial");
            if (m != null) {
                int.TryParse(m.Trim(), out etebar);
                return etebar;
            }
            return etebar;
        }
    }
}