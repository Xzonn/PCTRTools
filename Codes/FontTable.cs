using NARCFileReadingDLL;
using System;
using System.IO;
using System.Linq;

namespace PokemonCTR
{
    class FontTable
    {
        static readonly int ChineseCharStart = 0x01FE;
        static readonly string ChinesePunctuation = "…、，。？！：；《》（）—～·「」『』";
        public readonly IFontTable Table;
        public readonly Generation.Gen Gen;

        public FontTable(byte[] bytes)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(bytes));
            int header = br.ReadInt32();
            br.BaseStream.Position = 0;
            switch (header)
            {
                case 0x10:
                    Table = new SimpleFontTable(br);
                    Gen = Generation.Gen.Gen4;
                    break;
                case 0x4E465452:
                    Table = new NFTRNitroFile(br);
                    Gen = Generation.Gen.Gen5;
                    break;
            }
            br.Close();
        }

        public bool Export(string path)
        {
            try
            {
                for (int i = 1; i < Table.Items.Length; i++)
                {
                    DrawChar.ValuesToBitmap(Table.Items[i - 1].Item).Save(Path.Combine(path, $"{i:X4}.png"));
                }
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public byte[] Save(CharTable charTable, StyleType style = StyleType.BOTTOM_RIGHT, FontType font = FontType.SONG_TI)
        {
            switch (Gen)
            {
                case Generation.Gen.Gen4:
                    while (Table.Items.Length <= charTable.maxCharCode)
                    {
                        Table.AddNewItem();
                    }
                    for (ushort i = 1; i <= charTable.maxCharCode; i++)
                    {
                        char c = charTable.GetCharacter(i);
                        if (ChinesePunctuation.Contains(c))
                        {
                            Table.Items[i - 1].Item = DrawChar.CharToValues(c, style, FontType.MS_GOTHIC, posX: -2 + ("？！".Contains(c) ? -3 : 0), posY: 2);
                            Table.Items[i - 1].Width = 13;
                        }
                        else if (i >= ChineseCharStart)
                        {
                            Table.Items[i - 1].Item = DrawChar.CharToValues(c, style, font);
                            Table.Items[i - 1].Width = 13;
                        }
                    }
                    break;
                case Generation.Gen.Gen5:
                    NFTRNitroFile tempTable = (NFTRNitroFile)Table;
                    foreach (char c in ChinesePunctuation)
                    {
                        ushort code = tempTable[c];
                        if (code > 0 && code < tempTable.Items.Length)
                        {
                            CGLPFrame.Character item = (CGLPFrame.Character)tempTable.Items[code];
                            switch (font)
                            {
                                case FontType.PIXEL_9:
                                    item.Width = (byte)(((style == StyleType.BOTTOM_RIGHT_5) ? 9 : 10) - item.SpaceWidth);
                                    break;
                                default:
                                    item.Width = (byte)(12 - item.SpaceWidth);
                                    break;
                            }
                            item.SpaceWidth = 0;
                        }
                    }
                    CMAPFrame lastFrame = (CMAPFrame)tempTable.Frames[tempTable.FramesCount - 1];
                    char[] originalChars = lastFrame.Keys, newChars = charTable.Values;
                    ushort[] originalValues = lastFrame.Values, newValues = charTable.Keys;
                    for (ushort i = 0, j = 0; i < originalChars.Length && j < newChars.Length; i++)
                    {
                        if (originalChars[i] >= 0x4E00 && originalChars[i] < 0xE000)
                        {
                            while (j < newChars.Length && (newChars[j] < 0x4E00 || newChars[j] >= 0xE000))
                            {
                                j++;
                            }
                            if (j < newChars.Length)
                            {
                                lastFrame[originalValues[i]] = newChars[j];
                                CGLPFrame.Character item = (CGLPFrame.Character)tempTable.Items[originalValues[i]];
                                switch (font)
                                {
                                    case FontType.PIXEL_9:
                                        item.Item = DrawChar.CharToValues(newChars[j], style, font, posX: -1, posY: -1);
                                        item.Width = (byte)((style == StyleType.BOTTOM_RIGHT_5) ? 9 : 10);
                                        break;
                                    default:
                                        item.Item = DrawChar.CharToValues(newChars[j], style, font);
                                        item.Width = 12;
                                        break;
                                }
                                item.SpaceWidth = 0;
                                j++;
                            }
                        }
                    }
                    break;
                default:
                    return new byte[0];
            }
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            Table.WriteTo(bw);
            byte[] bytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(bytes, 0, bytes.Length);
            bw.Close();
            return bytes;
        }
    }
}
