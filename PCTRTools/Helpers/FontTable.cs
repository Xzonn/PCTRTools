using NARCFileReadingDLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCTRTools;

internal class FontTable
{
  const int CHINESE_CHAR_START = 0x01F0;
  const byte CHINESE_CHAR_WIDTH = 12;
  private class CharConfig
  {
    public bool Draw = true;
    public bool MS_Gothic = true;
    public char DrawWith = '\0';
    public int PosX = -2, PosY = 2;
  }

  static readonly Dictionary<char, CharConfig> ChinesePunctuationConfig = new()
  {
    ['　'] = new CharConfig(),
    ['…'] = new CharConfig(),
    ['、'] = new CharConfig(),
    ['，'] = new CharConfig() { PosY = 3 },
    ['。'] = new CharConfig(),
    ['？'] = new CharConfig() { PosX = -4, PosY = 3 },
    ['！'] = new CharConfig() { PosX = -4, PosY = 3 },
    ['：'] = new CharConfig() { PosX = -5, PosY = 3 },
    ['；'] = new CharConfig() { PosX = -5, PosY = 3 },
    ['《'] = new CharConfig() { PosX = -4 },
    ['》'] = new CharConfig() { PosX = 0 },
    ['（'] = new CharConfig() { PosX = -3 },
    ['）'] = new CharConfig() { PosX = -1 },
    ['—'] = new CharConfig() { MS_Gothic = false, DrawWith = '一', PosY = 1 },
    ['～'] = new CharConfig() { MS_Gothic = false, PosX = -1 },
    ['“'] = new CharConfig() { PosY = 1 },
    ['”'] = new CharConfig() { PosY = 1 },
  };
  public readonly IFontTable Table;
  public readonly Generation.Gen Gen;

  public FontTable(byte[] bytes)
  {
    BinaryReader br = new(new MemoryStream(bytes));
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

  public byte[] Save(CharTable charTable, DrawChar.StyleType style = DrawChar.StyleType.BOTTOM_RIGHT, DrawChar.FontType font = DrawChar.FontType.SONG_TI)
  {
    switch (Gen)
    {
      case Generation.Gen.Gen4:
        byte charWidth = (byte)(CHINESE_CHAR_WIDTH + ((style == DrawChar.StyleType.ROUND) ? 1 : 0));
        while (Table.Items.Length <= charTable.maxCharCode)
        {
          Table.AddNewItem();
        }
        Table.Items[0x01fb - 1].Width = charWidth;
        for (ushort i = 1; i <= charTable.maxCharCode; i++)
        {
          char c = charTable.GetCharacter(i);
          if (ChinesePunctuationConfig.Keys.Contains(c))
          {
            var config = ChinesePunctuationConfig[c];
            if (config.Draw)
            {
              Table.Items[i - 1].Item = DrawChar.CharToValues(config.DrawWith != '\0' ? config.DrawWith : c, style, config.MS_Gothic ? DrawChar.FontType.MS_GOTHIC : font, config.PosX, config.PosY);
            }
            Table.Items[i - 1].Width = charWidth;
          }
          else if (i >= CHINESE_CHAR_START)
          {
            Table.Items[i - 1].Item = DrawChar.CharToValues(c, style, font);
            Table.Items[i - 1].Width = charWidth;
          }
        }
        break;
      case Generation.Gen.Gen5:
        NFTRNitroFile tempTable = (NFTRNitroFile)Table;
        foreach (var pair in ChinesePunctuationConfig)
        {
          char c = pair.Key;
          if (c == '　') { continue; }
          var config = pair.Value;
          ushort code = tempTable[c];
          if (code > 0 && code < tempTable.Items.Length)
          {
            CGLPFrame.Character item = (CGLPFrame.Character)tempTable.Items[code];
            switch (font)
            {
              case DrawChar.FontType.PIXEL_9:
                // item.Width = (byte)(((style == DrawChar.StyleType.BOTTOM_RIGHT_5) ? 9 : 10) - item.SpaceWidth);
                break;
              default:
                item.Item = DrawChar.CharToValues(config.DrawWith != '\0' ? config.DrawWith : c, style, config.MS_Gothic ? DrawChar.FontType.MS_GOTHIC : font, config.PosX, config.PosY);
                item.SpaceWidth = 0;
                item.Width = CHINESE_CHAR_WIDTH;
                item.SpaceAfter = 0;
                break;
            }
          }
        }
        CMAPFrame lastFrame = (CMAPFrame)tempTable.Frames[tempTable.FramesCount - 1];
        char[] originalChars = lastFrame.Keys, newChars = charTable.Values.OrderBy(c => c).ToArray();
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
                case DrawChar.FontType.PIXEL_9:
                  item.Item = DrawChar.CharToValues(newChars[j], style, font, posX: -1, posY: -1);
                  item.Width = (byte)((style == DrawChar.StyleType.BOTTOM_RIGHT_5) ? 9 : 10);
                  break;
                default:
                  item.Item = DrawChar.CharToValues(newChars[j], style, font);
                  item.Width = CHINESE_CHAR_WIDTH;
                  break;
              }
              item.SpaceWidth = 0;
              item.SpaceAfter = 0;
              j++;
            }
          }
        }
        break;
      default:
        return new byte[0];
    }
    using (MemoryStream ms = new())
    {
      using (BinaryWriter bw = new(ms))
      {
        Table.WriteTo(bw);
        return ms.ToArray();
      }
    }
  }
}
