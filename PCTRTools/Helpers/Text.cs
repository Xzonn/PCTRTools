using System;
using System.Collections.Generic;
using System.IO;

namespace PCTRTools;

internal class Text
{
  public readonly List<List<List<string>>> TextList = new();
  public int Version = 0;
  public Generation.Gen Gen = Generation.Gen.Gen4;

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
      var tw = File.CreateText(path);
      if (this is TextGen4 && Version > 0)
      {
        tw.WriteLine($"#{Version}");
      }
      for (int i = 0; i < TextList.Count; i++)
      {
        for (int j = 0; j < TextList[i].Count; j++)
        {
          if (TextList[i][j].Count == 0) { continue; }
          tw.WriteLine($"{j}" + (this is TextGen5 ? $"-{i}" : ""));
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
    Version = 0;
    try
    {
      string[] all = File.ReadAllLines(path);
      int currentBin = 0, currentType = 0;
      foreach (string line in all)
      {
        if (line == "") { continue; }
        if (line[0] == '#')
        {
          Version = Convert.ToInt32(line.Substring(1));
          continue;
        }
        string[] splited = line.Split('\t');
        string[] subline = splited[0].Split('-');
        int j = Convert.ToInt32(subline[0]);
        if (splited.Length == 1)
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
          TextList[currentType][currentBin][j] = splited[1];
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
  public void Save(ref NARC msg, CharTable charTable)
  {
    if (this is TextGen4 gen4)
    {
      gen4.Save(ref msg, charTable);
    }
    else if (this is TextGen5 gen5)
    {
      gen5.Save(ref msg);
    }
  }
}

