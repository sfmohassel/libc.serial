using System;
using System.IO;
using System.Xml.Serialization;

namespace libc.serial
{
  public abstract class XmlFile
  {
    [XmlIgnore]
    public string FilePath;

    public abstract object CreateDefault();

    public static T Load<T>(string filePath) where T : XmlFile, new()
    {
      if (!File.Exists(filePath))
      {
        //create
        var res = new T().CreateDefault() as T;
        res.FilePath = filePath;
        XML.WriteToXmlFile(filePath, res);

        return res;
      }

      try
      {
        var k = XML.ReadFromXmlFile<T>(filePath);
        k.FilePath = filePath;

        return k;
      }
      catch (Exception)
      {
        //create
        var res = new T().CreateDefault() as T;
        res.FilePath = filePath;
        XML.WriteToXmlFile(filePath, res);

        return res;
      }
    }

    public void Save()
    {
      XML.WriteToXmlFile(FilePath, this);
    }
  }
}