using System;
using System.Collections.Generic;
using libc.concurrency;
using libc.serial.Base;
namespace libc.serial.WithThreshold {
    public class ComPortWithThreshold : ComPort {
        private readonly WorkerThread checkBufferThread;
        private readonly Func<List<byte>, bool> packetRcv;
        private readonly ComPortWithThresholdSettings settings;
        public ComPortWithThreshold(ComPortWithThresholdSettings settings,
            Func<List<byte>, bool> packetRcvAction,
            Action<ComPortErrorNames, Exception> errorCallback)
            : base(settings, errorCallback) {
            try {
                this.settings = settings;
                packetRcv = packetRcvAction ?? throw new ArgumentNullException(nameof(packetRcvAction));
                Buffer = new Queue<byte>();
                checkBufferThread = new WorkerThread(checkBuffer, 1);
                checkBufferThread.Start();
            } catch (Exception ex) {
                raiseError(ComPortErrorNames.OnConstruction, ex);
            }
        }
        public override void Dispose() {
            base.Dispose();
            try {
                checkBufferThread.Stop();
                clearBuffer();
            } catch (Exception ex) {
                raiseError(ComPortErrorNames.OnDispose, ex);
            }
        }
        private void checkBuffer() {
            List<byte> packet;
            var count = settings.Threshold;
            lock (Buffer) {
                if (Buffer.Count < count) return;
                packet = new List<byte>();
                for (var i = 0; i < count; i++) packet.Add(Buffer.Dequeue());
            }
            var valid = packetRcv(packet);
            if (!valid) clearBuffer();
        }
        private void clearBuffer() {
            TryClearInBuffer();
            lock (Buffer) {
                Buffer.Clear();
            }
        }
    }
}