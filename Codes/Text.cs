using System;
using System.Collections.Generic;
using System.IO;

namespace PokemonCTR
{
    class Text
    {
        public readonly List<List<string>> TextList = new List<List<string>>();
        private readonly List<int> OriginalKeys = new List<int>();

        /// <summary>
        /// 从 narc 文件根据码表读取文本。
        /// </summary>
        /// <param name="files">narc 文件</param>
        /// <param name="charTable">码表</param>
        public Text (Narc files, CharTable charTable)
        {
            foreach (byte[] bytes in files.Files)
            {
                List<string> s = new List<string>();
                BinaryReader br = new BinaryReader(new MemoryStream(bytes));
                bool flag2 = false;
                int control = 0;
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
                    num = (0x91bd3 * (j + 1)) & 0xffff;
                    string text = "";
                    for (int k = 0; k < numArray2[j]; k++)
                    {
                        int num4 = br.ReadUInt16();
                        num4 ^= num;
                        switch (num4)
                        {
                            case 0xE000:
                                text += "\\n";
                                break;
                            case 0x25BC:
                                text += "\\r";
                                break;
                            case 0x25BD:
                                text += "\\f";
                                break;
                            case 0xF100:
                                flag2 = true;
                                break;
                            case 0xFFFE:
                                text += "[";
                                control = 3;
                                break;
                            case 0xFFFF:
                                break;
                            default:
                                if (control > 0)
                                {
                                    text += Convert.ToString(num4, 16).ToUpper().PadLeft(4, '0');
                                    control--;
                                    if (control == 0)
                                    {
                                        text += "]";
                                    }
                                }
                                else
                                {
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
                                                            text += "\\x" + Convert.ToString(num8, 16).PadLeft(4, '0');
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
                                                        text += "\\x" + Convert.ToString(num8, 16).PadLeft(4, '0');
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
                                            text += "\\x" + Convert.ToString(num4, 16).PadLeft(4, '0');
                                        }
                                        else
                                        {
                                            text += str3;
                                        }
                                    }
                                }
                                break;
                        }
                        num += 18749;
                        num &= 65535;
                    }
                    s.Add(text);
                }
                TextList.Add(s);
            }
        }

        /// <summary>
        /// 将文本导出为 txt 文件。
        /// </summary>
        /// <param name="path">txt 文件路径</param>
        /// <returns>是否导出成功</returns>
        public bool Extract (string path)
        {
            try
            {
                TextWriter tw = File.CreateText(path);
                for (int i = 0; i < TextList.Count; i++)
                {
                    tw.WriteLine($"{i}");
                    for (int j = 0; j < TextList[i].Count; j++)
                    {
                        tw.WriteLine($"{j}\t{TextList[i][j]}");
                    }
                }
                tw.Close();
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 从 txt 文件导入文本。
        /// </summary>
        /// <param name="path">txt 文件路径</param>
        /// <returns>是否导入成功</returns>
        public bool Import (string path)
        {
            try
            {
                string[] all = File.ReadAllLines(path);
                int current = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].Length > 0)
                    {
                        string[] line = all[i].Split('\t');
                        int j = Convert.ToInt32(line[0]);
                        if (line.Length == 1)
                        {
                            current = j;
                        }
                        else
                        {
                            TextList[current][j] = line[1];
                        }
                    }
                }
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 将文本根据保存为 narc 文件。
        /// </summary>
        /// <param name="files">narc 文件</param>
        /// <param name="charTable">码表</param>
        public void Save (ref Narc files, CharTable charTable)
        {
            for (int i = 0; i < TextList.Count; i++)
            {
                List<string> subList = TextList[i];
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
                        size += 4;
                        i += 13;
                        break;
                    default:
                        size++;
                        break;
                }
            }
            size++;
            return size;
        }

        private static int[] EncodeText(string str, int stringSize, CharTable charTable)
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
                                numArray[index] = 0x25BC;
                                i++;
                                break;
                            case 'f':
                                numArray[index] = 0x25BD;
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
                        numArray[index++] = 0xFFFE;
                        numArray[index++] = Convert.ToInt32(str.Substring(i + 1, 4), 16);
                        numArray[index++] = Convert.ToInt32(str.Substring(i + 5, 4), 16);
                        numArray[index] = Convert.ToInt32(str.Substring(i + 9, 4), 16);
                        i += 13;
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
