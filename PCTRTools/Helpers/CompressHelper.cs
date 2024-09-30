using System;
using System.Collections.Generic;

namespace PCTRTools;

// Credit to: DarthNemesis
// https://sourceforge.net/projects/darthnemesis/
internal class CompressHelper
{
  public static byte[] CompressLzss(byte[] uncompressedData)
  {
    return Compress(uncompressedData, 18, 16, 1);
  }

  private static byte[] Compress(byte[] uncompressedData, int readAheadBufferSize, byte compressionType, int distance)
  {
    List<byte> list = new();
    Queue<byte> queue = new(readAheadBufferSize);
    List<byte> list2 = new(4096);
    int i = 0;
    list.Add(compressionType);
    list.AddRange(BitConverter.GetBytes(uncompressedData.Length));
    list.RemoveAt(4);


    for (; i < readAheadBufferSize; i++)
    {
      queue.Enqueue(uncompressedData[i]);
    }

    while (queue.Count > 0)
    {
      bool[] array = new bool[8];
      List<byte[]> list3 = new();
      for (int num = 7; num >= 0; num--)
      {
        int[] array2 = Search(list2, queue.ToArray(), distance);
        if (array2[1] > 2)
        {
          array[num] = true;
          byte[] array3 = new byte[2]
          {
            (byte)(((array2[1] - (readAheadBufferSize - 15)) & 0xF) << 4),
            0
          };
          array3[0] += (byte)((array2[0] - distance >> 8) & 0xF);
          array3[1] = (byte)((uint)(array2[0] - distance) & 0xFFu);
          list3.Add(array3);
        }
        else if (array2[1] >= 0)
        {
          array2[1] = 1;
          byte[] array3 = new byte[1] { queue.Peek() };
          array[num] = false;
          list3.Add(array3);
        }
        else
        {
          array[num] = false;
        }

        for (int j = 0; j < array2[1]; j++)
        {
          if (list2.Count >= 4096)
          {
            list2.RemoveAt(0);
          }

          list2.Add(queue.Dequeue());
        }

        while (queue.Count < readAheadBufferSize && i < uncompressedData.Length)
        {
          queue.Enqueue(uncompressedData[i]);
          i++;
        }
      }

      byte b = 0;
      for (int k = 0; k < 8; k++)
      {
        if (array[k])
        {
          b = (byte)(b + (byte)(1 << k));
        }
      }

      list.Add(b);
      foreach (byte[] item2 in list3)
      {
        byte[] array4 = item2;
        foreach (byte item in array4)
        {
          list.Add(item);
        }
      }
    }

    return list.ToArray();
  }

  private static int[] Search(List<byte> slidingWindow, byte[] readAheadBuffer, int distance)
  {
    if (readAheadBuffer.Length == 0)
    {
      return new int[2] { 0, -1 };
    }

    List<int> list = new();
    for (int i = 0; i < slidingWindow.Count - distance; i++)
    {
      if (slidingWindow[i] == readAheadBuffer[0])
      {
        list.Add(i);
      }
    }

    if (list.Count == 0)
    {
      return new int[2];
    }

    for (int j = 1; j < readAheadBuffer.Length; j++)
    {
      for (int k = 0; k < list.Count; k++)
      {
        if (slidingWindow[list[k] + j % (slidingWindow.Count - list[k])] != readAheadBuffer[j] && list.Count > 1)
        {
          list.Remove(list[k]);
          k--;
        }
      }

      if (list.Count < 2)
      {
        j = readAheadBuffer.Length;
      }
    }

    int num = 1;
    bool flag = true;
    while (readAheadBuffer.Length > num && flag)
    {
      if (slidingWindow[list[0] + num % (slidingWindow.Count - list[0])] == readAheadBuffer[num])
      {
        num++;
      }
      else
      {
        flag = false;
      }
    }

    return new int[2]
    {
      slidingWindow.Count - list[0],
      num
    };
  }
}
