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
                bool flag = false, flag2 = false;
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
                        if (num4 == 57344 || num4 == 9660 || num4 == 9661 || num4 == 61696 || num4 == 65534 || num4 == 65535)
                        {
                            if (num4 == 57344)
                                text += "\\n";
                            if (num4 == 9660)
                                text += "\\r";
                            if (num4 == 9661)
                                text += "\\f";
                            if (num4 == 61696)
                                flag2 = true;
                            if (num4 == 65534)
                            {
                                text += "\\v";
                                flag = true;
                            }
                        }
                        else
                        {
                            if (flag)
                            {
                                text += Convert.ToString(num4, 16).PadLeft(4, '0');
                                flag = false;
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
                                                    } else
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
                                                } else
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
                                    } else
                                    {
                                        text += str3;
                                    }
                                }
                            }
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
                BinaryWriter br = new BinaryWriter(ms);
                br.Write((ushort)subList.Count);
                br.Write((ushort)OriginalKeys[i]);
                int num = (OriginalKeys[i] * 0x2fd) & 0xffff, num4 = 4 + (subList.Count * 8);
                int[] numArray = new int[subList.Count];
                for (int j = 0; j < subList.Count; j++)
                {
                    int num2 = (num * (j + 1)) & 0xffff;
                    int num3 = num2 | (num2 << 16);
                    br.Write((uint)(num4 ^ num3));
                    numArray[j] = GetRealSize(subList[j]);
                    br.Write((uint)(GetRealSize(subList[j]) ^ num3));
                    num4 += GetRealSize(subList[j]) * 2;
                }
                for (int k = 0; k < subList.Count; k++)
                {
                    num = (0x91bd3 * (k + 1)) & 0xffff;
                    int[] numArray2 = EncodeText(subList[k], numArray[k], charTable);
                    for (int l = 0; l < numArray[k] - 1; l++)
                    {
                        br.Write((short)(numArray2[l] ^ num));
                        num += 0x493d;
                        num &= 0xffff;
                    }
                    br.Write((short)(0xffff ^ num));
                }
                byte[] bytes = new byte[ms.Position];
                ms.Position = 0;
                ms.Read(bytes, 0, bytes.Length);
                files.Files[i] = bytes;
                br.Close();
            }
        }

        private static int GetRealSize(string p)
        {
            int size = 0;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == '\\')
                {
                    if (p[i + 1] == 'r')
                    {
                        size++;
                        i++;
                    }
                    else if (p[i + 1] == 'n')
                    {
                        size++;
                        i++;
                    }
                    else if (p[i + 1] == 'f')
                    {
                        size++;
                        i++;
                    }
                    else if (p[i + 1] == 'v')
                    {
                        size += 2;
                        i += 5;
                    }
                    else
                    {
                        size++;
                        i += 5;
                    }
                }
                else
                {
                    size++;
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
                if (str[i] == '\\')
                {
                    if (str[i + 1] == 'r')
                    {
                        numArray[index] = 0x25bc;
                        i++;
                    }
                    else if (str[i + 1] == 'n')
                    {
                        numArray[index] = 0xe000;
                        i++;
                    }
                    else if (str[i + 1] == 'n')
                    {
                        numArray[index] = 0xe000;
                        i++;
                    }
                    else if (str[i + 1] == 'f')
                    {
                        numArray[index] = 0x25bd;
                        i++;
                    }
                    else if (str[i + 1] == 'v')
                    {
                        numArray[index] = 0xfffe;
                        index++;
                        numArray[index] = Convert.ToInt32(str.Substring(i + 2, 4), 16);
                        i += 5;
                    }
                    else if ((str[i + 1] == 'x') && (str[i + 2] == '0') && (str[i + 3] == '0') && (str[i + 4] == '0') && (str[i + 5] == '0'))
                    {
                        numArray[index] = 0;
                        i += 5;
                    }
                    else if ((str[i + 1] == 'x') && (str[i + 2] == '0') && (str[i + 3] == '0') && (str[i + 4] == '0') && (str[i + 5] == '1'))
                    {
                        numArray[index] = 1;
                        i += 5;
                    }
                    else
                    {
                        numArray[index] = Convert.ToInt32(str.Substring(i + 2, 4), 16);
                        i += 5;
                    }
                }
                else
                {
                    numArray[index] = charTable.WriteCharacter(str[i]);
                }
                index++;
            }
            return numArray;
        }
    }
}
