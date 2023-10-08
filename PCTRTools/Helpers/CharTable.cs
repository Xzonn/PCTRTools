using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCTRTools
{
  internal class CharTable
  {
    private readonly Dictionary<ushort, char> _charTableDict = new Dictionary<ushort, char>();
    public readonly List<char> NoCode = new List<char>();
    public readonly List<ushort> NoChar = new List<ushort>();
    public ushort maxCharCode;

    /// <summary>
    /// 从码表文件路径读取码表。
    /// </summary>
    /// <param name="path">码表文件路径</param>
    public CharTable(string path)
    {
      if (File.Exists(path))
      {
        string[] CharTableText = File.ReadAllText(path, Encoding.UTF8).Split('\n');
        foreach (string s in CharTableText)
        {
          string[] ss = s.Split('\t');
          if (ss.Length > 1)
          {
            ushort i = Convert.ToUInt16(ss[0], 16);
            char c = ss[1][0];
            _charTableDict[i] = c;
            maxCharCode = i > maxCharCode ? i : maxCharCode;
          }
        }
      }
    }

    /// <summary>
    /// 将编码转换为字符。
    /// </summary>
    /// <param name="i">编码</param>
    /// <returns></returns>
    public char GetCharacter(ushort i)
    {
      if (_charTableDict.ContainsKey(i))
      {
        return _charTableDict[i];
      }
      else
      {
        if (i > 0 && !NoChar.Contains(i))
        {
          NoChar.Add(i);
        }
        return '\0';
      }
    }

    public char GetCharacter(int i)
    {
      return GetCharacter((ushort)i);
    }

    /// <summary>
    /// 将字符转换为编码。
    /// </summary>
    /// <param name="c">字符</param>
    /// <returns></returns>
    public int WriteCharacter(char c)
    {
      if (_charTableDict.ContainsValue(c))
      {
        return _charTableDict.First(x => x.Value == c).Key;
      }
      else
      {
        if (c > 0 && !NoCode.Contains(c))
        {
          NoCode.Add(c);
        }
        return 0;
      }
    }

    public ushort[] Keys
    {
      get
      {
        return _charTableDict.Keys.ToArray();
      }
    }

    public char[] Values
    {
      get
      {
        return _charTableDict.Values.ToArray();
      }
    }
  }
}
