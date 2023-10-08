using System;
using System.Collections.Generic;
using System.IO;

namespace PCTRTools
{
  class TextGen4 : Text
  {
    private readonly List<int> OriginalKeys = new List<int>();

    new public Generation.Gen Gen
    {
      get
      {
        return Generation.Gen.Gen4;
      }
    }

    public TextGen4(NARC files, CharTable charTable)
    {
      Version = 3;
      TextList.Clear();
      TextList.Add(new List<List<string>>());
      foreach (byte[] bytes in files.Files)
      {
        BinaryReader br = new BinaryReader(new MemoryStream(bytes));
        List<string> s4 = new List<string>();
        bool flag2 = false;
        int stringCount = br.ReadUInt16(), originalKey = br.ReadUInt16();
        OriginalKeys.Add(originalKey);
        int num = (originalKey * 0x2fd) & 0xffff;
        int[] numArray = new int[stringCount];
        int[] numArray2 = new int[stringCount];
        for (int i = 0; i < stringCount; i++)
        {
          int num2 = (num * (i + 1)) & 0xffff;
          int num3 = num2 | (num2 << 16);
          numArray[i] = br.ReadInt32();
          numArray[i] = numArray[i] ^ num3;
          numArray2[i] = br.ReadInt32();
          numArray2[i] = numArray2[i] ^ num3;
        }
        for (int j = 0; j < stringCount; j++)
        {
          int control = 0;
          num = (0x91bd3 * (j + 1)) & 0xffff;
          string text = "";
          for (int k = 0; k < numArray2[j]; k++)
          {
            int num4 = br.ReadUInt16();
            num4 ^= num;
            switch (control)
            {
              case 0:
                switch (num4)
                {
                  case 0xE000:
                    text += "\\n";
                    break;
                  case 0x25BC:
                    if (Version > 2)
                    {
                      text += "\\f";
                    }
                    else
                    {
                      text += "\\r";
                    }
                    break;
                  case 0x25BD:
                    if (Version > 2)
                    {
                      text += "\\r";
                    }
                    else
                    {
                      text += "\\f";
                    }
                    break;
                  case 0xF100:
                    flag2 = true;
                    break;
                  case 0xFFFE:
                    text += "[";
                    control = 1;
                    break;
                  case 0xFFFF:
                    break;
                  default:
                    if (flag2)
                    {
                      int num5 = 0;
                      int num6 = 0;
                      string str = null;
                      while (true)
                      {
                        if (num5 >= 15)
                        {
                          num5 -= 15;
                          if (num5 > 0)
                          {
                            int num8 = num6 | (num4 << 9 - num5 & 511);
                            if ((num8 & 255) == 255)
                              break;
                            if (num8 != 0 && num8 != 1)
                            {
                              char str2 = charTable.GetCharacter(num8);
                              if (str2 == '\0')
                              {
                                text += "\\x" + num8.ToString("X4");
                              }
                              else
                              {
                                text += str2;
                              }
                            }
                          }
                        }
                        else
                        {
                          int num8 = num4 >> num5 & 511;
                          if ((num8 & 255) == 255)
                            break;
                          if (num8 != 0 && num8 != 1)
                          {
                            char str3 = charTable.GetCharacter(num8);
                            if (str3 == '\0')
                            {
                              text += "\\x" + num8.ToString("X4");
                            }
                            else
                            {
                              text += str3;
                            }
                          }
                          num5 += 9;
                          if (num5 < 15)
                          {
                            num6 = num4 >> num5 & 511;
                            num5 += 9;
                          }
                          num += 18749;
                          num &= 65535;
                          num4 = br.ReadUInt16();
                          num4 ^= num;
                          k++;
                        }
                      }
                      text += str;
                    }
                    else
                    {
                      char str3 = charTable.GetCharacter(num4);
                      if (str3 == '\0')
                      {
                        text += "\\x" + num4.ToString("X4");
                      }
                      else
                      {
                        text += str3;
                      }
                    }
                    break;
                }
                break;
              case 1:
                text += num4.ToString("X4");
                if (num4 == 0x0129) // 不知道为何，很奇怪的控制符
                {
                  control = 3;
                }
                else
                {
                  control = 2;
                }
                break;
              case 2:
                control = -num4;
                if (control == 0)
                {
                  text += "]";
                }
                break;
              case 3:
                control = -num4 + 1;
                if (control == 0)
                {
                  text += "]";
                }
                break;
              default:
                text += "," + num4.ToString("X4");
                control++;
                if (control == 0)
                {
                  text += "]";
                }
                break;
            }
            num += 18749;
            num &= 65535;
          }
          s4.Add(text);
        }
        TextList[0].Add(s4);
      }
    }

    new public void Save(ref NARC files, CharTable charTable)
    {
      for (int i = 0; i < files.Files.Count; i++)
      {
        List<string> subList = TextList[0][i];
        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((ushort)subList.Count);
        bw.Write((ushort)OriginalKeys[i]);
        int num = (OriginalKeys[i] * 0x2fd) & 0xffff, num4 = 4 + (subList.Count * 8);
        int[] numArray = new int[subList.Count];
        for (int j = 0; j < subList.Count; j++)
        {
          int num2 = (num * (j + 1)) & 0xffff;
          int num3 = num2 | (num2 << 16);
          bw.Write((uint)(num4 ^ num3));
          numArray[j] = GetRealSize(subList[j]);
          bw.Write((uint)(GetRealSize(subList[j]) ^ num3));
          num4 += GetRealSize(subList[j]) * 2;
        }
        for (int k = 0; k < subList.Count; k++)
        {
          num = (0x91bd3 * (k + 1)) & 0xffff;
          int[] numArray2 = EncodeText(subList[k], numArray[k], charTable);
          for (int l = 0; l < numArray[k] - 1; l++)
          {
            bw.Write((short)(numArray2[l] ^ num));
            num += 0x493d;
            num &= 0xffff;
          }
          bw.Write((short)(0xffff ^ num));
        }
        byte[] bytes = new byte[ms.Position];
        ms.Position = 0;
        ms.Read(bytes, 0, bytes.Length);
        files.Files[i] = bytes;
        bw.Close();
      }
    }

    private static int GetRealSize(string p)
    {
      int size = 0;
      for (int i = 0; i < p.Length; i++)
      {
        switch (p[i])
        {
          case '\\':
            switch (p[i + 1])
            {
              case 'r':
              case 'f':
              case 'n':
                size++;
                i++;
                break;
              case 'v':
                size += 2;
                i += 5;
                break;
              default:
                size++;
                i += 5;
                break;
            }
            break;
          case '[':
            int rightPos = p.IndexOf(']', i);
            string[] controlText = p.Substring(i + 1, rightPos - i - 1).Split(',');
            size += controlText.Length + 2;
            i = rightPos;
            break;
          default:
            size++;
            break;
        }
      }
      size++;
      return size;
    }

    private int[] EncodeText(string str, int stringSize, CharTable charTable)
    {
      int[] numArray = new int[stringSize - 1];
      int index = 0;
      for (int i = 0; i < str.Length; i++)
      {
        switch (str[i])
        {
          case '\\':
            switch (str[i + 1])
            {
              case 'r':
                if (Version > 2)
                {
                  numArray[index] = 0x25BD;
                }
                else
                {
                  numArray[index] = 0x25BC;
                }
                i++;
                break;
              case 'f':
                if (Version > 2)
                {
                  numArray[index] = 0x25BC;
                }
                else
                {
                  numArray[index] = 0x25BD;
                }
                i++;
                break;
              case 'n':
                numArray[index] = 0xE000;
                i++;
                break;
              case 'v':
                numArray[index++] = 0xFFFE;
                numArray[index] = Convert.ToInt32(str.Substring(i + 2, 4), 16);
                i += 5;
                break;
              case 'x':
                numArray[index] = Convert.ToInt32(str.Substring(i + 2, 4), 16);
                i += 5;
                break;
            }
            break;
          case '[':
            int rightPos = str.IndexOf(']', i);
            string[] controlText = str.Substring(i + 1, rightPos - i - 1).Split(',');
            numArray[index++] = 0xFFFE;
            numArray[index++] = Convert.ToInt32(controlText[0], 16);
            if (controlText[0] == "0129")
            {
              numArray[index++] = controlText.Length - 1 + 1;
            }
            else
            {
              numArray[index++] = controlText.Length - 1;
            }
            for (int j = 1; j < controlText.Length; j++)
            {
              numArray[index++] = Convert.ToInt32(controlText[j], 16);
            }
            index--;
            i = rightPos;
            break;
          default:
            numArray[index] = charTable.WriteCharacter(str[i]);
            break;
        }
        index++;
      }
      return numArray;
    }
  }
}
