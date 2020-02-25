using NARCFileReadingDLL;
using System;
using System.Drawing;

namespace PokemonCTR
{
    public enum StyleType
    {
        OUT,
        IN
    }

    public enum FontType : int
    {
        SONG_TI = 0,
        MS_GOTHIC = 1
    }

    class DrawChar
    {

        static readonly Color[] Colors = { Color.FromArgb(0, 255, 255, 0), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(0, 255, 0, 0) };
        static readonly Font[] Fonts = { new Font("新宋体", 12, GraphicsUnit.Pixel), new Font("MS Gothic", 12, GraphicsUnit.Pixel) };

        static public Bitmap ValuesToBitmap(VALUE[,] v, int w = 16, int h = 16)
        {
            Bitmap b = new Bitmap(w, h);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    b.SetPixel(x, y, Colors[(int)v[y, x]]);
                }
            }
            return b;
        }
        static public VALUE[,] CharToValues(char c, StyleType type = StyleType.OUT, FontType fontType = FontType.SONG_TI, int posX = -2, int posY = 1)
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.DrawString(c.ToString(), Fonts[(int)fontType], new SolidBrush(Color.Black), new Point(posX, posY));
            int x, y;
            VALUE[,] v = new VALUE[16, 16];
            for (x = 0; x < 16; x++)
                for (y = 0; y < 16; y++)
                    v[y, x] = (type == StyleType.OUT ? VALUE.VALUE_3 : VALUE.VALUE_0);
            switch (type)
            {
                case StyleType.OUT:
                    for (x = 0; x < 15; x++)
                    {
                        for (y = 0; y < 15; y++)
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
                case StyleType.IN:
                    for (x = 15; x > -1; x--)
                    {
                        for (y = 14; y > -1; y--)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y + 1, x] = VALUE.VALUE_1;
                                v[y, x] = VALUE.VALUE_3;
                                if (x > 0)
                                {
                                    v[y + 1, x - 1] = VALUE.VALUE_3;
                                    v[y, x - 1] = VALUE.VALUE_3;
                                }
                            }
                        }
                    }
                    break;
            }
            return v;
        }

        static public Size ValuesToSize(VALUE[,] v, StyleType type = StyleType.OUT, int w = 16, int h = 16)
        {
            int x, y;
            int minX, maxX = w - 1, minY = 0, maxY = h - 1;
            for (minX = 0; minX < w; minX++)
            {
                for (y = 0; y < h; y++)
                {
                    if ((type == StyleType.OUT && v[y, minX] != VALUE.VALUE_3) || (type == StyleType.IN && v[y, minX] != VALUE.VALUE_0))
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
                    if ((type == StyleType.OUT && v[y, maxX] != VALUE.VALUE_3) || (type == StyleType.IN && v[y, maxX] != VALUE.VALUE_0))
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
                    if ((type == StyleType.OUT && v[minY, x] != VALUE.VALUE_3) || (type == StyleType.IN && v[minY, x] != VALUE.VALUE_0))
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
                    if ((type == StyleType.OUT && v[maxY, x] != VALUE.VALUE_3) || (type == StyleType.IN && v[maxY, x] != VALUE.VALUE_0))
                    {
                        break;
                    }
                }
                if (x < w)
                {
                    break;
                }
            }
            Console.WriteLine(maxX);
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
}
