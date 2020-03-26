using System;
using System.Collections.Generic;
using System.IO;

namespace PokemonCTR
{
    class Text
    {
        public readonly List<List<List<string>>> TextList = new List<List<List<string>>>();
        public Generation.Gen Gen;

        /// <summary>
        /// 从 narc 文件根据码表读取文本。
        /// </summary>
        /// <param name="files">narc 文件</param>
        /// <param name="charTable">码表</param>
        public Text() { }

        /// <summary>
        /// 将文本导出为 txt 文件。
        /// </summary>
        /// <param name="path">txt 文件路径</param>
        /// <returns>是否导出成功</returns>
        public bool Extract(string path)
        {
            try
            {
                TextWriter tw = File.CreateText(path);
                for (int i = 0; i < TextList.Count; i++)
                {
                    for (int j = 0; j < TextList[i].Count; j++)
                    {
                        tw.WriteLine($"{j}" + (this is Text_5 ? $"-{i}" : ""));
                        for (int k = 0; k < TextList[i][j].Count; k++)
                        {
                            tw.WriteLine($"{k}\t{TextList[i][j][k]}");
                        }
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
        public bool Import(string path)
        {
            try
            {
                string[] all = File.ReadAllLines(path);
                int currentBin = 0, currentType = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].Length > 0)
                    {
                        string[] line = all[i].Split('\t');
                        string[] subline = line[0].Split('-');
                        int j = Convert.ToInt32(subline[0]);
                        if (line.Length == 1)
                        {
                            currentBin = j;
                            if (subline.Length == 2)
                            {
                                currentType = Convert.ToInt32(subline[1]);
                            }
                            else
                            {
                                currentType = 0;
                            }
                        }
                        else
                        {
                            TextList[currentType][currentBin][j] = line[1];
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
        public void Save() { }
    }
}
