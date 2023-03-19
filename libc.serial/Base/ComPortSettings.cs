using System.IO.Ports;

namespace libc.serial.Base
{
  public class ComPortSettings : XmlFile
  {
    public string PortName { get; set; }
    public int BaudRate { get; set; }
    public int DataBits { get; set; }
    public Parity Parity { get; set; }
    public StopBits StopBits { get; set; }
    public Handshake Handshake { get; set; }
    public int ReadBufferSize { get; set; }
    public int WriteBufferSize { get; set; }

    public virtual bool Validate()
    {
      return !string.IsNullOrWhiteSpace(PortName) && BaudRate > 0 && DataBits > 0 && ReadBufferSize > 0 &&
             WriteBufferSize > 0;
    }

    public override object CreateDefault()
    {
      var res = new ComPortSettings
      {
        PortName = "COM1",
        BaudRate = 38400,
        DataBits = 8,
        Parity = Parity.None,
        StopBits = StopBits.One,
        Handshake = Handshake.None,
        ReadBufferSize = 1024 * 1024,
        WriteBufferSize = 1024 * 1024
      };

      return res;
    }
  }
}