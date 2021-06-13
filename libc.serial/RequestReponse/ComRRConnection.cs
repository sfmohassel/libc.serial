using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using libc.serial.Internal;
using libc.serial.Resources;
using libc.serial.WithThreshold;

namespace libc.serial.RequestReponse
{
    public abstract class ComRrConnection
    {
        private static readonly Hashtable cache = new Hashtable();
        private readonly Action<ComRrConnection> finished;
        private readonly object lck = new object();
        private readonly ComPortWithThreshold port;
        private readonly ComPortWithThresholdSettings portSettings;
        private readonly byte requestHeader;
        private readonly int requestLength;
        private readonly byte responseHeader;
        private readonly int responseLength;
        private readonly DelayedTask timeoutTask;

        protected ComRrConnection(ComRRRequest request,
            int requestLength,
            byte requestHeader,
            int responseLength,
            byte responseHeader,
            int timeout,
            object info,
            ComPortWithThreshold port,
            ComPortWithThresholdSettings portSettings,
            Action<ComRrConnection> onDone)
        {
            Request = request;
            this.requestLength = requestLength;
            this.requestHeader = requestHeader;
            this.responseLength = responseLength;
            this.responseHeader = responseHeader;
            Responses = new List<ComRRResponse>();
            this.port = port;
            this.portSettings = portSettings;
            finished = onDone;
            Info = info;
            timeoutTask = new DelayedTask(onTimeout, timeout);
            State = ComRRConnectionStates.None;
            ResponsePackets = new List<List<byte>>();
        }

        public ComRRRequest Request { get; }
        public List<ComRRResponse> Responses { get; }
        public List<List<byte>> ResponsePackets { get; }
        public ComRRConnectionStates State { get; private set; }
        public ComRRConnectionErrors Error { get; set; }
        public string Message { get; private set; }
        protected object Info { get; }

        public T GetInfo<T>() where T : class
        {
            return Info as T;
        }

        public void Start()
        {
            lock (lck)
            {
                if (State == ComRRConnectionStates.None)
                {
                    try
                    {
                        var packet = new byte[requestLength];
                        packet[0] = requestHeader;
                        var data = new byte[packet.Length - 2];
                        EncodeRequestInData(data);
                        data.CopyTo(packet, 1);
                        packet[packet.Length - 1] = (byte) ((byte) data.Sum(a => a) + packet[0]);
                        //update threshold and timeout
                        portSettings.Threshold = responseLength;
                        Request.PacketBytes = packet;
                        port.Write(packet, true);
                        State = ComRRConnectionStates.Sent;
                        timeoutTask.Start();
                    }
                    catch (Exception ex)
                    {
                        error(ex.ToString(), ComRRConnectionErrors.Exception);
                    }
                }
                else
                {
                    var msg = $"{nameof(State)} is {State} but required state is: {ComRRConnectionStates.None}";
                    error(msg, ComRRConnectionErrors.InvalidState);
                }
            }
        }

        public bool PacketRcv(List<byte> packet)
        {
            var res = false;

            lock (lck)
            {
                ResponsePackets.Add(packet);

                if (State == ComRRConnectionStates.Sent)
                {
                    var cks = (byte) packet.Take(packet.Count - 1).Sum(a => a);

                    if (packet.Count != responseLength)
                    {
                        //length mismatch
                        var msg =
                            $"received packet length is {packet.Count} but required length is: {responseLength}";

                        error(msg, ComRRConnectionErrors.InvalidResponseLenght);
                    }
                    else if (packet[0] != responseHeader)
                    {
                        //invalid header
                        var msg =
                            $"received packet header is {packet[0]} but required header is: {responseHeader}";

                        error(msg, ComRRConnectionErrors.InvalidResponseHeader);
                    }
                    else if (packet[packet.Count - 1] != cks)
                    {
                        //invalid checksum
                        var msg =
                            $"received packet check sum is {packet[packet.Count - 1]} but requried check sum is: {cks}";

                        error(msg, ComRRConnectionErrors.InvalidResponseChecksum);
                    }
                    else
                    {
                        var data = packet.Skip(1).Take(packet.Count - 2).ToArray();
                        var response = DecodeResponseFromData(data);
                        AfterDecodeResponseFromData(response);
                        res = true;
                    }
                }
                else
                {
                    var msg =
                        $"{nameof(State)} is {State} but required state is: {ComRRConnectionStates.Sent} in PacketRcv";

                    error(msg, ComRRConnectionErrors.InvalidState);
                }
            }

            return res;
        }

        public static ComRrConnection Create(Type serialConnectionType,
            ComRRRequest request,
            ComPortWithThreshold port,
            ComPortWithThresholdSettings portSettings,
            object info,
            Action<ComRrConnection> onFinished,
            int requestLength,
            byte requestHeader,
            int responseLength,
            byte responseHeader,
            int timeout)
        {
            var n = serialConnectionType.FullName;

            var ciObj = cache[n] ?? (cache[n] = serialConnectionType.GetConstructor(new[]
            {
                typeof(ComRRRequest),
                typeof(int),
                typeof(byte),
                typeof(int),
                typeof(byte),
                typeof(int),
                typeof(object),
                typeof(ComPortWithThreshold),
                typeof(ComPortWithThresholdSettings),
                typeof(Action<ComRrConnection>)
            }));

            var ci = ciObj as ConstructorInfo;

            if (ci == null)
                throw new Exception(
                    $"serial connection wrapper constructor not found!!! conenction type: {serialConnectionType}");

            if (info == null)
                throw new Exception($"serial connection info not found!!! conenction type: {serialConnectionType}");

            var res = (ComRrConnection) ci.Invoke(new[]
            {
                request,
                requestLength,
                requestHeader,
                responseLength,
                responseHeader,
                timeout,
                info,
                port,
                portSettings,
                onFinished
            });

            return res;
        }

        protected void Done(string unCompletedMessage = null)
        {
            lock (lck)
            {
                timeoutTask.Stop();

                if (unCompletedMessage == null)
                {
                    State = ComRRConnectionStates.Done;
                    Message = "تکمیل";
                }
                else
                {
                    State = ComRRConnectionStates.DoneButNotComplete;
                    Message = unCompletedMessage;
                }

                finished?.BeginInvoke(this, null, null);
            }
        }

        protected abstract void EncodeRequestInData(byte[] data);

        protected abstract ComRRResponse DecodeResponseFromData(byte[] data);

        protected abstract void AfterDecodeResponseFromData(ComRRResponse newResponse);

        protected abstract bool IsTimeoutError();

        private void onTimeout()
        {
            lock (lck)
            {
                if (State == ComRRConnectionStates.Sent)
                    if (IsTimeoutError())
                    {
                        State = ComRRConnectionStates.Timedout;
                        Message = Tran.Instance.Get("Timeout");
                        finished?.BeginInvoke(this, null, null);
                    }
            }
        }

        private void error(string msg, ComRRConnectionErrors error)
        {
            State = ComRRConnectionStates.Error;
            Error = error;
            Message = msg;
            finished?.BeginInvoke(this, null, null);
        }
    }
}