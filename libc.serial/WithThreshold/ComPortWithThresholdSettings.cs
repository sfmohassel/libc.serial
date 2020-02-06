using System.Xml.Serialization;
using libc.serial.Base;
using Newtonsoft.Json;
namespace libc.serial.WithThreshold {
    public class ComPortWithThresholdSettings : ComPortSettings {
        [JsonIgnore]
        [XmlIgnore]
        private readonly object thresholdLock = new object();
        [JsonIgnore]
        [XmlIgnore]
        private int threshold;
        public int Threshold {
            get {
                lock (thresholdLock) {
                    return threshold;
                }
            }
            set {
                lock (thresholdLock) {
                    threshold = value;
                }
            }
        }
        public override bool Validate() {
            return base.Validate() && Threshold > 1;
        }
        public override object CreateDefault() {
            var baseSettings = base.CreateDefault() as ComPortSettings;
            var res = new ComPortWithThresholdSettings {
                BaudRate = baseSettings.BaudRate,
                DataBits = baseSettings.DataBits,
                Handshake = baseSettings.Handshake,
                Parity = baseSettings.Parity,
                PortName = baseSettings.PortName,
                ReadBufferSize = baseSettings.ReadBufferSize,
                StopBits = baseSettings.StopBits,
                WriteBufferSize = baseSettings.WriteBufferSize,
                Threshold = 8
            };
            return res;
        }
    }
}