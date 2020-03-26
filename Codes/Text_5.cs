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
                for (int i = 0; i < numSections; i++)
                {
                    List<string> s = new List<string>();
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
                                            line += $",{encText[j][k + 2 + control]:X4}";
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
                    TextList[i].Add(s);
                }
            }
        }

        public void Save(ref Narc files, CharTable charTable)
        {
            for (int i = 0; i < files.Files.Count; i++)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                // ？？？

                byte[] bytes = new byte[ms.Position];
                ms.Position = 0;
                ms.Read(bytes, 0, bytes.Length);
                files.Files[i] = bytes;
                bw.Close();
            }
        }
    }
}
