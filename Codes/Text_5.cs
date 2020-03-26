using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PokemonCTR
{
    class Text_5 : Text
    {
        private readonly List<ushort> Keys = new List<ushort>();
        private readonly List<ushort> Unknowns = new List<ushort>();
        new public Generation.Gen Gen
        {
            get
            {
                return Generation.Gen.Gen5;
            }
        }

        public Text_5(Narc files, CharTable charTable)
        {
            TextList.Clear();
            TextList.Add(new List<List<string>>());
            TextList.Add(new List<List<string>>());
            foreach (byte[] bytes in files.Files)
            {
                BinaryReader br = new BinaryReader(new MemoryStream(bytes));
                ushort numSections, numEntries, tmpCharCount, tmpUnknown, tmpChar;
                uint unk1, tmpOffset;
                uint[] sizeSections = new uint[] { 0, 0 };
                uint[] sectionOffset = new uint[] { 0, 0 };
                ushort key;
                numSections = br.ReadUInt16();
                numEntries = br.ReadUInt16();
                sizeSections[0] = br.ReadUInt32();
                unk1 = br.ReadUInt32();
                for (int i = 0; i < numSections; i++)
                {
                    sectionOffset[i] = br.ReadUInt32();
                }
                for (int i = 0; i < TextList.Count; i++)
                {
                    List<string> s = new List<string>();
                    if (i < numSections)
                    {
                        br.BaseStream.Position = sectionOffset[i];
                        sizeSections[i] = br.ReadUInt32();

                        List<uint> tableOffsets = new List<uint>();
                        List<ushort> characterCount = new List<ushort>();
                        List<ushort> unknown = new List<ushort>();
                        List<List<ushort>> encText = new List<List<ushort>>();
                        for (int j = 0; j < numEntries; j++)
                        {
                            tmpOffset = br.ReadUInt32();
                            tmpCharCount = br.ReadUInt16();
                            tmpUnknown = br.ReadUInt16();
                            tableOffsets.Add(tmpOffset);
                            characterCount.Add(tmpCharCount);
                            unknown.Add(tmpUnknown);
                            Unknowns.Add(tmpUnknown);
                        }
                        for (int j = 0; j < numEntries; j++)
                        {
                            List<ushort> tmpEncChars = new List<ushort>();
                            br.BaseStream.Position = sectionOffset[i] + tableOffsets[j];
                            for (int k = 0; k < characterCount[j]; k++)
                            {
                                tmpChar = br.ReadUInt16();
                                tmpEncChars.Add(tmpChar);
                            }
                            encText.Add(tmpEncChars);
                            key = (ushort)(encText[j][characterCount[j] - 1] ^ 0xFFFF);
                            for (int k = characterCount[j] - 1; k >= 0; k--)
                            {
                                encText[j][k] ^= key;
                                if (k == 0)
                                {
                                    Keys.Add(key);
                                }
                                key = (ushort)(((key >> 3) | (key << 13)) & 0xFFFF);
                            }
                            string line = "";
                            for (int k = 0; k < characterCount[j]; k++)
                            {
                                if (encText[j][k] > 20 && encText[j][k] <= 0xFFF0 && encText[j][k] != 0xF000)
                                {
                                    line += (char)encText[j][k];
                                }
                                else if (encText[j][k] == 0xFFFE)
                                {
                                    line += "\\n";
                                }
                                else if (encText[j][k] == 0xFFFF)
                                {
                                    // line += "\\xffff";
                                }
                                else if (encText[j][k] == 0xF000)
                                {
                                    switch (encText[j][k + 1])
                                    {
                                        case 0xBE00:
                                            line += "\\r";
                                            break;
                                        case 0xBE01:
                                            line += "\\f";
                                            break;
                                        default:
                                            line += $"[{encText[j][k + 1]:X4}";
                                            for (int control = 0; control < encText[j][k + 2]; control++)
                                            {
                                                line += $",{encText[j][k + 3 + control]:X4}";
                                            }
                                            line += "]";
                                            break;
                                    }
                                    k += 2 + encText[j][k + 2];
                                }
                                else
                                {
                                    line += $"\\x{encText[j][k]:X4}";
                                }
                            }
                            s.Add(line);
                        }
                    }
                    TextList[i].Add(s);
                }
            }
        }

        public void Save(ref Narc files)
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
                sizeSections[0] = br.ReadUInt32();
                uint unk1 = br.ReadUInt32();

                List<byte[]> newEntries = new List<byte[]>();
                for (int j = 0; j < numSections; j++)
                {
                    newEntries.Add(MakeSection(TextList[j][i], numEntries));
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
                bw.Write(newsizeSections[0]);
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

                byte[] bytes = new byte[ms.Position];
                ms.Position = 0;
                ms.Read(bytes, 0, bytes.Length);
                files.Files[i] = bytes;
                bw.Close();
                br.Close();
            }
        }

        private byte[] MakeSection (List<string> s, int numEntries)
        {
            List<List<ushort>> data = new List<List<ushort>>();
            uint size = 0, offset = (uint)(4 + 8 * numEntries);
            ushort charCount, unk1 = 0x100;
            for (int k = 0; k < numEntries; k++)
            {
                data.Add(EncodeText(s[k], k));
                size += (uint)(data[k].Count * 2);
            }
            if (size % 4 == 2)
            {
                size += 2;
                ushort tmpKey = Keys[numEntries - 1];
                for (int k = 0; k < data[numEntries - 1].Count; k++)
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
                bws.Write(Unknowns[k]);
                offset += (ushort)(charCount * 2);
            }
            for (int k = 0; k < numEntries; k++)
            {
                for (int l = 0; l < data[k].Count; l++)
                {
                    bws.Write(data[k][l]);
                }
            }
            byte[] section = new byte[mss.Position];
            mss.Position = 0;
            mss.Read(section, 0, section.Length);
            mss.Close();
            return section;
        }

        private List<ushort> EncodeText(string str, int id)
        {
            List<ushort> chars = new List<ushort>();
            for (int i = 0; i < str.Length; i++)
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
                                    chars.Add(Convert.ToUInt16(str.Substring(i + 2, 4), 16));
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
            ushort key = Keys[id];
            for (int i = 0; i < chars.Count; i++)
            {
                chars[i] ^= key;
                key = (ushort)(((key << 3) | (key >> 13)) & 0xFFFF);
            }
            return chars;
        }
    }
}
