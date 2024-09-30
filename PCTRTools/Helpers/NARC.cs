using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCTRTools;

internal class NARC
{
  private class NarcHeader
  {
    public uint id;
    public ushort id0;
    public ushort id1;
    public uint file_size;
    public ushort length;
    public ushort num_sections;
  }

  private class FatbHeader
  {
    public uint id;
    public uint length;
    public uint num_entries;
  }

  private class FatbEntry
  {
    public uint file_from_offset;
    public uint file_to_offset;
  }

  private class FntbHeader
  {
    public uint id;
    public uint length;
    public uint unknown0;
    public uint unknown1;
  }

  private class FntbEntry
  {
    public byte name_length;
    public char[] name;
  }

  private class FimgHeader
  {
    public uint id;
    public uint length;
  }

  public readonly List<byte[]> Files = new();
  public readonly List<char[]> FileNames = new();
  private readonly NarcHeader narcHeader;
  private readonly FatbHeader fatbHeader;
  private readonly List<FatbEntry> fatbEntries;
  private readonly FntbHeader fntbHeader;
  private readonly FimgHeader fimgHeader;

  /// <summary>
  /// 从 narc 文件路径读取 narc 文件。
  /// </summary>
  /// <param name="path">narc 文件路径</param>
  public NARC(string path)
  {
    if (File.Exists(path))
    {
      BinaryReader br = new(File.OpenRead(path));
      int i;
      narcHeader = new NarcHeader
      {
        id = br.ReadUInt32(),
        id0 = br.ReadUInt16(),
        id1 = br.ReadUInt16(),
        file_size = br.ReadUInt32(),
        length = br.ReadUInt16(),
        num_sections = br.ReadUInt16()
      };
      fatbHeader = new FatbHeader
      {
        id = br.ReadUInt32(),
        length = br.ReadUInt32(),
        num_entries = br.ReadUInt32()
      };
      fatbEntries = new List<FatbEntry>();
      for (i = 0; i < fatbHeader.num_entries; i++)
      {
        fatbEntries.Add(new FatbEntry
        {
          file_from_offset = br.ReadUInt32(),
          file_to_offset = br.ReadUInt32()
        });
      }
      fntbHeader = new FntbHeader
      {
        id = br.ReadUInt32(),
        length = br.ReadUInt32(),
        unknown0 = br.ReadUInt32(),
        unknown1 = br.ReadUInt32()
      };
      if (fntbHeader.unknown0 == 8) // 原文为 unknown1
      {
        for (i = 0; i < fatbHeader.num_entries; i++)
        {
          byte name_length = br.ReadByte();
          char[] name = br.ReadChars(name_length);
          FileNames.Add(name);
        }
      }
      br.BaseStream.Position = narcHeader.length + fatbHeader.length + fntbHeader.length;
      fimgHeader = new FimgHeader
      {
        id = br.ReadUInt32(),
        length = br.ReadUInt32()
      };
      long positon = br.BaseStream.Position;
      for (i = 0; i < fatbHeader.num_entries; i++)
      {
        uint fileLength = fatbEntries[i].file_to_offset - fatbEntries[i].file_from_offset;
        br.BaseStream.Position = positon + fatbEntries[i].file_from_offset;
        Files.Add(br.ReadBytes((int)fileLength));
      }
      br.Close();
    }
  }

  public bool Save(string path)
  {
    try
    {
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      BinaryWriter bw = new(File.Create(path));
      int i;
      uint position = 0;
      fatbHeader.num_entries = (uint)Files.Count;
      fatbHeader.length = 0x0C + 8 * fatbHeader.num_entries;
      fatbEntries.Clear();
      for (i = 0; i < Files.Count; i++)
      {
        var entry = new FatbEntry
        {
          file_from_offset = position,
          file_to_offset = position + (uint)Files[i].Length
        };
        fatbEntries.Add(entry);
        position += (uint)Files[i].Length;
        position = (position + 3) / 4 * 4;
      }
      byte[] fntbBytes = Array.Empty<byte>();
      if (fntbHeader.unknown0 == 8)
      {
        MemoryStream ms2 = new();
        BinaryWriter bw2 = new(ms2);
        for (i = 0; i < Files.Count; i++)
        {
          bw2.Write((byte)FileNames[i].Length);
          bw2.Write(FileNames[i], 0, (byte)FileNames[i].Length);
        }
        bw.Write((byte)0);
      }
      fntbHeader.length = (uint)(0x10 + fntbBytes.Length + 0x0F) / 0x10 * 0x10;
      fimgHeader.length = position + 8;
      narcHeader.file_size = narcHeader.length + fatbHeader.length + fntbHeader.length + fimgHeader.length;
      /* NarcHeader */
      bw.Write(narcHeader.id);
      bw.Write(narcHeader.id0);
      bw.Write(narcHeader.id1);
      bw.Write(narcHeader.file_size);
      bw.Write(narcHeader.length);
      bw.Write(narcHeader.num_sections);
      /* FatbHeader */
      bw.Write(fatbHeader.id);
      bw.Write(fatbHeader.length);
      bw.Write(fatbHeader.num_entries);
      /* FatbEntries */
      for (i = 0; i < fatbHeader.num_entries; i++)
      {
        bw.Write(fatbEntries[i].file_from_offset);
        bw.Write(fatbEntries[i].file_to_offset);
      }
      /* FntbHeader */
      bw.Write(fntbHeader.id);
      bw.Write(fntbHeader.length);
      bw.Write(fntbHeader.unknown0);
      bw.Write(fntbHeader.unknown1);
      /* FntbEntries */
      if (fntbHeader.unknown0 == 8)
      {
        bw.Write(fntbBytes);
        bw.Write(Enumerable.Repeat((byte)0xFF, (int)(bw.BaseStream.Position - (narcHeader.length + fatbHeader.length + fntbHeader.length))).ToArray());
      }
      bw.BaseStream.Position = narcHeader.length + fatbHeader.length + fntbHeader.length;
      /* FimgHeader */
      bw.Write(fimgHeader.id);
      bw.Write(fimgHeader.length);
      /* Files */
      for (i = 0; i < fatbHeader.num_entries; i++)
      {
        bw.Write(Files[i]);
        bw.Write(Enumerable.Repeat((byte)0xFF, (int)(4 - bw.BaseStream.Position % 4) % 4).ToArray());
      }
      bw.Close();
      return true;
    }
    catch (IOException ex)
    {
      Console.WriteLine(ex.Message);
      return false;
    }
  }
}
