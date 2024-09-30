using NARCFileReadingDLL;
using System.Collections.Generic;
using System.Drawing;

namespace PCTRTools;

internal class DrawChar
{
  public enum StyleType
  {
    BOTTOM_RIGHT = 0,
    TOP_LEFT = 1,
    ROUND = 2,
    BOTTOM_RIGHT_5 = 3
  }

  public enum FontType : int
  {
    SONG_TI = 0,
    HEI_TI = 1,
    MS_GOTHIC = 2,
    PIXEL_9 = 3
  }

  static readonly Color[] Colors = { Color.FromArgb(0, 255, 255, 0), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 128, 128, 128), Color.FromArgb(128, 255, 0, 0) };
  static readonly Dictionary<FontType, Font> Fonts = new()
  {
          { FontType.SONG_TI, new Font("新宋体", 12, GraphicsUnit.Pixel) },
          { FontType.HEI_TI, new Font("黑体", 12, GraphicsUnit.Pixel) },
          { FontType.MS_GOTHIC, new Font("MS Gothic", 12, GraphicsUnit.Pixel) },
          { FontType.PIXEL_9, new Font("Zfull-GB", 9, GraphicsUnit.Pixel) }
      };

  static public Bitmap ValuesToBitmap(VALUE[,] v)
  {
    int w = v.GetLength(1), h = v.GetLength(0);
    Bitmap b = new(w, h);
    for (int x = 0; x < w; x++)
    {
      for (int y = 0; y < h; y++)
      {
        b.SetPixel(x, y, Colors[(int)v[y, x]]);
      }
    }
    return b;
  }
  static public VALUE[,] CharToValues(char c, StyleType type = StyleType.BOTTOM_RIGHT, FontType fontType = FontType.SONG_TI, int posX = -2, int posY = 1, int w = 16, int h = 16)
  {
    Bitmap b = new(w, h);
    Graphics g = Graphics.FromImage(b);
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
    g.DrawString(c.ToString(), Fonts[fontType], new SolidBrush(Color.Black), new Point(posX, posY));
    int x, y;
    VALUE[,] v = new VALUE[w, h];
    for (x = 0; x < w; x++)
    {
      for (y = 0; y < h; y++)
      {
        v[y, x] = type == StyleType.BOTTOM_RIGHT ? VALUE.VALUE_3 : VALUE.VALUE_0;
      }
    }
    switch (type)
    {
      case StyleType.BOTTOM_RIGHT:
      case StyleType.BOTTOM_RIGHT_5:
        for (x = 0; x < w - 1; x++)
        {
          for (y = 0; y < h - 1; y++)
          {
            if (b.GetPixel(x, y).A > 200)
            {
              v[y, x] = VALUE.VALUE_1;
              v[y + 1, x] = VALUE.VALUE_2;
              v[y, x + 1] = VALUE.VALUE_2;
              v[y + 1, x + 1] = VALUE.VALUE_2;
            }
          }
        }
        break;
      case StyleType.TOP_LEFT:
        for (x = w - 1; x > -1; x--)
        {
          for (y = h - 2; y > -1; y--)
          {
            if (b.GetPixel(x, y).A > 200)
            {
              v[y + 1, x + 1] = VALUE.VALUE_1;
              v[y, x + 1] = VALUE.VALUE_3;
              v[y + 1, x] = VALUE.VALUE_3;
              v[y, x] = VALUE.VALUE_3;
            }
          }
        }
        break;
      case StyleType.ROUND:
        for (x = 0; x < w - 2; x++)
        {
          for (y = 0; y < h - 2; y++)
          {
            if (b.GetPixel(x, y).A > 200)
            {
              v[y + 1, x + 1] = VALUE.VALUE_1;
              v[y, x] = v[y, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x];
              v[y + 1, x] = v[y + 1, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y + 1, x];
              v[y + 2, x] = v[y + 2, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y + 2, x];
              v[y, x + 1] = v[y, x + 1] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x + 1];
              v[y + 2, x + 1] = VALUE.VALUE_2;
              v[y, x + 2] = v[y, x + 2] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x + 2];
              v[y + 1, x + 2] = VALUE.VALUE_2;
              v[y + 2, x + 2] = VALUE.VALUE_2;
            }
          }
        }
        break;
    }
    return v;
  }

  static public Size ValuesToSize(VALUE[,] v, StyleType type = StyleType.BOTTOM_RIGHT, int w = 16, int h = 16)
  {
    int x, y, minX, maxX, minY, maxY;
    for (minX = 0; minX < w; minX++)
    {
      for (y = 0; y < h; y++)
      {
        if ((type == StyleType.BOTTOM_RIGHT && v[y, minX] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[y, minX] != VALUE.VALUE_0))
        {
          break;
        }
      }
      if (y < h)
      {
        break;
      }
    }
    for (maxX = w - 1; maxX > -1; maxX--)
    {
      for (y = 0; y < h; y++)
      {
        if ((type == StyleType.BOTTOM_RIGHT && v[y, maxX] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[y, maxX] != VALUE.VALUE_0))
        {
          break;
        }
      }
      if (y < h)
      {
        break;
      }
    }
    for (minY = 0; minY < h; minY++)
    {
      for (x = 0; x < w; x++)
      {
        if ((type == StyleType.BOTTOM_RIGHT && v[minY, x] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[minY, x] != VALUE.VALUE_0))
        {
          break;
        }
      }
      if (x < w)
      {
        break;
      }
    }
    for (maxY = h - 1; maxY > -1; maxY--)
    {
      for (x = 0; x < w; x++)
      {
        if ((type == StyleType.BOTTOM_RIGHT && v[maxY, x] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[maxY, x] != VALUE.VALUE_0))
        {
          break;
        }
      }
      if (x < w)
      {
        break;
      }
    }
    if (maxX < minX)
    {
      maxX = minX - 1;
    }
    if (maxY < minY)
    {
      maxY = minY - 1;
    }
    return new Size(maxX - minX + 1, maxY - minY + 1);
  }
}
