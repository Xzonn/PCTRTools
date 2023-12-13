using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PCTRTools
{
  internal class TextGen5 : Text
  {
    private readonly List<List<List<ushort>>> Keys = new List<List<List<ushort>>>
    {
      new List<List<ushort>>(),
      new List<List<ushort>>()
    };
    private readonly List<List<List<ushort>>> Unknowns = new List<List<List<ushort>>>
    {
      new List<List<ushort>>(),
      new List<List<ushort>>()
    };
    private readonly List<List<byte>> Unknowns2 = new List<List<byte>>
    {
      new List<byte>(),
      new List<byte>()
    };
    private readonly List<List<List<ushort>>> OriginalLengths = new List<List<List<ushort>>>
    {
      new List<List<ushort>>(),
      new List<List<ushort>>()
    };
    new public Generation.Gen Gen
    {
      get
      {
        return Generation.Gen.Gen5;
      }
    }

    public TextGen5(NARC files)
    {
      TextList.Clear();
      TextList.Add(new List<List<string>>());
      TextList.Add(new List<List<string>>());
      foreach (byte[] bytes in files.Files)
      {
        BinaryReader br = new BinaryReader(new MemoryStream(bytes));
        uint[] sizeSections = { 0, 0 };
        uint[] sectionOffset = { 0, 0 };
        ushort numSections = br.ReadUInt16();
        ushort numEntries = br.ReadUInt16();
        br.ReadUInt32();
        br.ReadUInt32();
        for (int i = 0; i < numSections; i++)
        {
          sectionOffset[i] = br.ReadUInt32();
        }
        for (int i = 0; i < TextList.Count; i++)
        {
          List<string> strings = new List<string>();
          List<ushort> keys = new List<ushort>();
          List<ushort> unknowns = new List<ushort>();
          byte unk2 = 0;
          List<ushort> characterCount = new List<ushort>();
          if (i < numSections)
          {
            br.BaseStream.Position = sectionOffset[i];
            sizeSections[i] = br.ReadUInt32();

            List<uint> tableOffsets = new List<uint>();
            List<List<ushort>> encText = new List<List<ushort>>();
            for (int j = 0; j < numEntries; j++)
            {
              tableOffsets.Add(br.ReadUInt32());
              characterCount.Add(br.ReadUInt16());
              unknowns.Add(br.ReadUInt16());
            }
            for (int j = 0; j < numEntries; j++)
            {
              br.BaseStream.Position = sectionOffset[i] + tableOffsets[j];
              List<ushort> tmpEncChars = new List<ushort>();
              while (tmpEncChars.Count < characterCount[j])
              {
                tmpEncChars.Add(br.ReadUInt16());
              }
              encText.Add(tmpEncChars);
              ushort key = (ushort)(encText[j][characterCount[j] - 1] ^ 0xFFFF);
              for (int k = characterCount[j] - 1; k >= 0; k--)
              {
                encText[j][k] ^= key;
                if (k == 0)
                {
                  keys.Add(key);
                }
                key = (ushort)(((key >> 3) | (key << 13)) & 0xFFFF);
              }
              string line = "";
              for (int charPos = 0; charPos < characterCount[j]; charPos++)
              {
                if (encText[j][charPos] > 20 && encText[j][charPos] <= 0xFFF0 && encText[j][charPos] != 0xF000)
                {
                  line += (char)encText[j][charPos];
                }
                else if (encText[j][charPos] == 0xFFFE)
                {
                  line += "\\n";
                }
                else if (encText[j][charPos] == 0xFFFF)
                {
                  // line += "\\xFFFF";
                }
                else if (encText[j][charPos] == 0xF000)
                {
                  switch (encText[j][charPos + 1])
                  {
                    case 0xBE00:
                      line += "\\r";
                      break;
                    case 0xBE01:
                      line += "\\f";
                      break;
                    default:
                      line += $"[{encText[j][charPos + 1]:X4}";
                      for (int control = 0; control < encText[j][charPos + 2]; control++)
                      {
                        line += $",{encText[j][charPos + 3 + control]:X4}";
                      }
                      line += "]";
                      break;
                  }
                  charPos += 2 + encText[j][charPos + 2];
                }
                else
                {
                  line += $"\\x{encText[j][charPos]:X4}";
                }
              }
              strings.Add(line);
            }

            if (br.BaseStream.Position - sectionOffset[i] != sizeSections[i])
            {
              unk2 = br.ReadByte();
              Debug.Assert(unk2 == br.ReadByte());
            }
          }

          TextList[i].Add(strings);
          Keys[i].Add(keys);
          Unknowns[i].Add(unknowns);
          Unknowns2[i].Add(unk2);
          OriginalLengths[i].Add(characterCount);
        }
      }
    }

    public void Save(ref NARC files)
    {
      for (int i = 0; i < files.Files.Count; i++)
      {
        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);
        BinaryReader br = new BinaryReader(new MemoryStream(files.Files[i]));

        uint[] sizeSections = { 0, 0 };
        uint[] sectionOffset = { 0, 0 };
        uint[] newsizeSections = { 0, 0 };

        ushort numSections = br.ReadUInt16(), numEntries = br.ReadUInt16();
        br.ReadUInt32();
        uint unk1 = br.ReadUInt32();

        List<byte[]> newEntries = new List<byte[]>();
        for (int j = 0; j < numSections; j++)
        {
          newEntries.Add(MakeSection(TextList[j][i], j, i, numEntries));
          newsizeSections[j] = (uint)newEntries[j].Length;
        }
        for (int j = 0; j < numSections; j++)
        {
          sectionOffset[j] = br.ReadUInt32();
        }
        for (int j = 0; j < numSections; j++)
        {
          br.BaseStream.Position = sectionOffset[j];
          sizeSections[j] = br.ReadUInt32();
        }
        bw.Write(numSections);
        bw.Write(numEntries);
        bw.Write(newsizeSections.Max());
        bw.Write(unk1);
        bw.Write(sectionOffset[0]);
        if (numSections == 2)
        {
          bw.Write(newsizeSections[0] + sectionOffset[0]);
        }
        for (int j = 0; j < numSections; j++)
        {
          bw.Write(newEntries[j]);
        }

        files.Files[i] = ms.ToArray();
        bw.Close();
        br.Close();
      }
    }

    private byte[] MakeSection(List<string> s, int numSection, int fileCount, int numEntries)
    {
      List<List<ushort>> data = new List<List<ushort>>();
      uint size = 0, offset = (uint)(4 + 8 * numEntries);
      ushort charCount;
      for (int k = 0; k < numEntries; k++)
      {
        data.Add(EncodeText(s[k], numSection, fileCount, k));
        size += (uint)(data[k].Count * 2);
      }
      if (size % 4 == 2)
      {
        size += 2;
        ushort tmpKey = Keys[numSection][fileCount][numEntries - 1];
        for (int charPos = 0; charPos < data[numEntries - 1].Count; charPos++)
        {
            tmpKey = (ushort)(((tmpKey << 3) | (tmpKey >> 13)) & 0xFFFF);
        }
        data[numEntries - 1].Add((ushort)(0xFFFF ^ tmpKey));
      }
      size += offset;

      MemoryStream mss = new MemoryStream();
      BinaryWriter bws = new BinaryWriter(mss);
      bws.Write(size);
      for (int k = 0; k < numEntries; k++)
      {
        charCount = (ushort)data[k].Count;
        bws.Write(offset);
        bws.Write(charCount);
        bws.Write(Unknowns[numSection][fileCount][k]);
        offset += (ushort)(charCount * 2);
      }
      for (int k = 0; k < numEntries; k++)
      {
        for (int l = 0; l < data[k].Count; l++)
        {
          bws.Write(data[k][l]);
        }
      }
      if (size != offset)
      {
        bws.Write(Unknowns2[numSection][fileCount]);
        bws.Write(Unknowns2[numSection][fileCount]);
      }
      return mss.ToArray();
    }

    private List<ushort> EncodeText(string str, int numSection, int fileCount, int numEntry)
    {
      List<ushort> chars = new List<ushort>();
      int i;
      for (i = 0; i < str.Length; i++)
      {
        switch (str[i])
        {
          case '\\':
            switch (str[i + 1])
            {
              case 'r':
                chars.Add(0xF000);
                chars.Add(0xBE00);
                chars.Add(0x0000);
                i++;
                break;
              case 'f':
                chars.Add(0xF000);
                chars.Add(0xBE01);
                chars.Add(0x0000);
                i++;
                break;
              case 'n':
                chars.Add(0xFFFE);
                i++;
                break;
              case 'x':
                if (str[i + 2] == '{')
                {
                  chars.Add('\\');
                  break;
                }
                else
                {
                  ushort code = Convert.ToUInt16(str.Substring(i + 2, 4), 16);
                  if (code != 0xFFFF)
                  {
                    chars.Add(code);
                  }
                  i += 5;
                  break;
                }
            }
            break;
          case '[':
            int rightPos = str.IndexOf(']', i);
            string[] controlText = str.Substring(i + 1, rightPos - i - 1).Split(',');
            chars.Add(0xF000);
            chars.Add(Convert.ToUInt16(controlText[0], 16));
            chars.Add((ushort)(controlText.Length - 1));
            for (int j = 1; j < controlText.Length; j++)
            {
              chars.Add(Convert.ToUInt16(controlText[j], 16));
            }
            i = rightPos;
            break;
          default:
            chars.Add(str[i]);
            break;
        }
      }
      chars.Add(0xFFFF);
      ushort key = Keys[numSection][fileCount][numEntry];
      for (i = 0; i < chars.Count; i++)
      {
        chars[i] ^= key;
        key = (ushort)(((key << 3) | (key >> 13)) & 0xFFFF);
      }
      return chars;
    }
  }
}
