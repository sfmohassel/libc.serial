using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace libc.serial.RequestReponse
{
  public abstract class ComRRRequest
  {
    protected ComRRRequest()
    {
      Identifier = Guid.NewGuid();
    }

    public Guid Identifier { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public object Tag { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public byte[] PacketBytes { get; set; }

    public abstract string GetPersianDesc();

    public override string ToString()
    {
      var res = new StringBuilder();
      res.AppendLine(GetPersianDesc());
      res.Append(Identifier);
      res.Append("__");
      String(res);

      return res.ToString();
    }

    protected abstract void String(StringBuilder res);
  }
}