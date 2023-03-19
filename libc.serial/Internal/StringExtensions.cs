using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libc.serial.Internal
{
  internal static class StringExtensions
  {
    internal static string ConcatString<T>(this IEnumerable<T> list,
        string delimiter, string startString = "", string endString = "")
    {
      var res = new StringBuilder("");
      var k = list?.ToList() ?? new List<T>();

      if (k.Any())
      {
        foreach (var o in k) res.AppendFormat("{0}{1}", o, delimiter);
        res.Remove(res.Length - delimiter.Length, delimiter.Length);
      }

      res.Insert(0, startString);
      res.Append(endString);

      return res.ToString();
    }

    internal static string StartingFrom(this string s, string from)
    {
      if (s == null || from == null) return null;
      var i = s.IndexOf(from);

      if (i >= 0)
        try
        {
          return s.Substring(i);
        }
        catch (ArgumentOutOfRangeException)
        {
          return null;
        }

      return null;
    }

    internal static string GetBetween(this string s, string first, string end)
    {
      if (s == null || first == null || end == null) return null;
      var a = s.IndexOf(first);

      if (a == -1) return null;
      var i1 = a + first.Length;
      var i2 = s.IndexOf(end, i1);

      if (i2 == -1 || i2 < i1) return null;
      if (i2 == i1) return string.Empty;
      var res = s.Substring(i1, i2 - i1);

      return res;
    }

    internal static string StartingAfter(this string s, string after)
    {
      if (s == null || after == null) return null;
      var i = s.IndexOf(after);

      if (i >= 0)
        try
        {
          return s.Substring(i + after.Length);
        }
        catch (ArgumentOutOfRangeException)
        {
          return null;
        }

      return null;
    }
  }
}