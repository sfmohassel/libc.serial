using libc.serial.Internal;
using libc.serial.Resources;
using libc.serial.WithThreshold;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace libc.serial.RequestReponse
{
  public abstract class ComRrConnection
  {
    private static readonly Hashtable _cache = new Hashtable();
    private readonly Action<ComRrConnection> _finished;
    private readonly object _lck = new object();
    private readonly ComPortWithThreshold _port;
    private readonly ComPortWithThresholdSettings _portSettings;
    private readonly byte _requestHeader;
    private readonly int _requestLength;
    private readonly byte _responseHeader;
    private readonly int _responseLength;
    private readonly DelayedTask _timeoutTask;

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
      _requestLength = requestLength;
      _requestHeader = requestHeader;
      _responseLength = responseLength;
      _responseHeader = responseHeader;
      Responses = new List<ComRRResponse>();
      _port = port;
      _portSettings = portSettings;
      _finished = onDone;
      Info = info;
      _timeoutTask = new DelayedTask(onTimeout, timeout);
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
      lock (_lck)
      {
        if (State == ComRRConnectionStates.None)
        {
          try
          {
            var packet = new byte[_requestLength];
            packet[0] = _requestHeader;
            var data = new byte[packet.Length - 2];
            EncodeRequestInData(data);
            data.CopyTo(packet, 1);
            packet[packet.Length - 1] = (byte)((byte)data.Sum(a => a) + packet[0]);
            //update threshold and timeout
            _portSettings.Threshold = _responseLength;
            Request.PacketBytes = packet;
            _port.Write(packet, true);
            State = ComRRConnectionStates.Sent;
            _timeoutTask.Start();
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

      lock (_lck)
      {
        ResponsePackets.Add(packet);

        if (State == ComRRConnectionStates.Sent)
        {
          var cks = (byte)packet.Take(packet.Count - 1).Sum(a => a);

          if (packet.Count != _responseLength)
          {
            //length mismatch
            var msg =
                $"received packet length is {packet.Count} but required length is: {_responseLength}";

            error(msg, ComRRConnectionErrors.InvalidResponseLenght);
          }
          else if (packet[0] != _responseHeader)
          {
            //invalid header
            var msg =
                $"received packet header is {packet[0]} but required header is: {_responseHeader}";

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

      var ciObj = _cache[n] ?? (_cache[n] = serialConnectionType.GetConstructor(new[]
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

      var res = (ComRrConnection)ci.Invoke(new[]
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
      lock (_lck)
      {
        _timeoutTask.Stop();

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

        _finished?.BeginInvoke(this, null, null);
      }
    }

    protected abstract void EncodeRequestInData(byte[] data);

    protected abstract ComRRResponse DecodeResponseFromData(byte[] data);

    protected abstract void AfterDecodeResponseFromData(ComRRResponse newResponse);

    protected abstract bool IsTimeoutError();

    private void onTimeout()
    {
      lock (_lck)
      {
        if (State == ComRRConnectionStates.Sent)
          if (IsTimeoutError())
          {
            State = ComRRConnectionStates.Timedout;
            Message = Tran.Instance.Get("Timeout");
            _finished?.BeginInvoke(this, null, null);
          }
      }
    }

    private void error(string msg, ComRRConnectionErrors error)
    {
      State = ComRRConnectionStates.Error;
      Error = error;
      Message = msg;
      _finished?.BeginInvoke(this, null, null);
    }
  }
}