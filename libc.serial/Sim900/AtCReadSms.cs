using libc.serial.AtDevice;
using libc.serial.Internal;
using System;
using System.Collections.Generic;

namespace libc.serial.Sim900
{
  public class AtCReadSms : AtCommand
  {
    private readonly string _cmd;

    public AtCReadSms(AtCReadSmsFlags flag)
    {
      var kkk = flag == AtCReadSmsFlags.All ? "ALL" : flag == AtCReadSmsFlags.Unread ? "REC UNREAD" : null;
      _cmd = $"at+cmgl=\"{kkk}\"\r";
      SmsList = new List<Sms>();
    }

    public List<Sms> SmsList { get; }

    public override string ToString()
    {
      return $"Command: {_cmd} - State: {State} - Sms list count: {SmsList.Count}";
    }

    protected override void RunCommandAsync()
    {
      port.Write(_cmd);
      setTimeout(20000);
    }

    protected override void OnDataUpdate(string buffer)
    {
      buffer = buffer.StartingFrom(_cmd);

      if (buffer == null) return;
      var bOk = buffer.GetBetween(_cmd, Ok);
      var bError = buffer.GetBetween(_cmd, Error);

      if (bError != null)
      {
        AddResponse(Error);
        finish(false, AtCommandStates.Completed);
      }
      else if (bOk != null)
      {
        AddResponse(Ok);
        readSmsList(buffer);
        finish(true, AtCommandStates.Completed);
      }
    }

    private void readSmsList(string buffer)
    {
      buffer = buffer.StartingAfter(_cmd);
      const string cmgl = "\r\n+CMGL: ";
      const string line = "\r\n";

      while (true)
      {
        buffer = buffer.StartingAfter(cmgl);

        if (buffer == null) break;
        var i = buffer.IndexOf(line);
        var data = buffer.Substring(0, i);
        buffer = buffer.StartingAfter(line);
        i = buffer.IndexOf(line);
        var text = buffer.Substring(0, i);
        readSms(data, text);
      }
    }

    private void readSms(string cmglData, string text)
    {
      var data = cmglData.Split(',');
      var m = data[2].GetBetween("\"", "\"");
      var mobile = m;
      if (m.StartsWith("+98")) mobile = "0" + m.Substring("+98".Length);
      var i = int.Parse(data[0].Trim());
      var f = data[1].ToLower().Contains("rec read") ? SmsFlags.Read : SmsFlags.Unread;
      var date = data[4].Substring(1);
      var year = int.Parse($"20{date.Substring(0, 2)}");
      var month = int.Parse(date.Substring(3, 2));
      var day = int.Parse(date.Substring(6, 2));
      var time = data[5].Substring(0, 8);
      var hour = int.Parse(time.Substring(0, 2));
      var minute = int.Parse(time.Substring(3, 2));
      var second = int.Parse(time.Substring(6, 2));

      SmsList.Add(new Sms
      {
        InboxIndex = i,
        Flag = f,
        Mobile = mobile,
        Text = text,
        SendDateTicks = new DateTimeOffset(new DateTime(year, month, day, hour, minute, second)).ToUnixTimeMilliseconds() * 1000
      });
    }
  }
}